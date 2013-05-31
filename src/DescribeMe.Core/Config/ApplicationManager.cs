﻿using System;
using System.Collections.Generic;
using System.Linq;
using DescribeMe.Core.DomainModels;
using DescribeMe.Core.Indexes;
using DescribeMe.Core.Services;
using Microsoft.Practices.ServiceLocation;
using NLog;
using Raven.Client;
using Raven.Abstractions.Data;
using DescribeMe.Core.Extensions;

namespace DescribeMe.Core.Config
{
    public class ApplicationManager : IApplicationManager
    {
        private readonly IDocumentStore _documentStore;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Application _application;
        private ICollection<Role> _roles;
        private readonly IConfigurationManager _configurationManager;

        public ApplicationManager(
            IDocumentStore documentStore,
            IConfigurationManager configurationManager)
        {
            _documentStore = documentStore;
            _configurationManager = configurationManager;
        }

        public ApplicationManager SetupApplication()
        {
            // Make sure that our users index is up to date or we may be inserting multiple admins/moderators.
            _documentStore.WaitForIndexingToFinish(new[]
                {
                    "Users/ByProvider"
                });

            using (var documentSession = _documentStore.OpenSession())
            {
                _application = documentSession.Load<Application>(Constants.ApplicationId);

                if (_application == null)
                {
                    AddApplication(documentSession);
                }
                else
                {
                    if (_application.DataImportRunning)
                        _application.DataImportFinished();
                }

                AddRoles(documentSession);

                AddAdmins(documentSession);

                AddModerators(documentSession);                

                documentSession.SaveChanges();
            }

            return this;
        }

        public ApplicationManager RegisterRavenWebsiteChanges()
        {
            var statisticsService = ServiceLocator.Current.GetInstance<IStatisticsService>();

            _documentStore.Changes()
                          .ForIndex("Images/Statistics")
                          .Subscribe(change =>
                          {
                              if (change.Type == IndexChangeTypes.ReduceCompleted)
                              {
                                  using (var documentSession = _documentStore.OpenSession())
                                  {
                                      RavenQueryStatistics stats;
                                      var statistics = documentSession
                                          .Query<Images_Statistics.ReduceResult, Images_Statistics>()
                                          .Statistics(out stats)
                                          .FirstOrDefault();
                                      
                                      if (!stats.IsStale)
                                      {
                                          log.Debug("Sending Statistics update to clients, {0} of {1} images described", statistics.DescibedImageCount, statistics.UnDescibedImageCount);

                                          statisticsService.UpdateStatistics(statistics);
                                      }
                                  }
                              }
                          });

            return this;
        }

        private void AddApplication(IDocumentSession documentSession)
        {
            _application = new Application();
            documentSession.Store(_application);
        }

        private void AddRoles(IDocumentSession documentSession)
        {
            _roles = new[]
                        {
                            new Role("administrator", "Administrator", "Administrator of Describe Me"),
                            new Role("moderator", "Moderator", "Moderators can review user descriptions"),
                            new Role("user", "Authenticated User", "User that has authenticated via 3rd party site")
                        };

            var existingRoles = documentSession
                .Query<Role>()
                .ToList();

            foreach (var role in _roles.Where(role => existingRoles.All(x => x.Id != role.Id)))
            {
                documentSession.Store(role);
            }

            documentSession.SaveChanges();
        }

        private void AddAdmins(IDocumentSession documentSession)
        {
            foreach (var admin in _configurationManager.GetAdmins())
            {
                var exisitingAdmin = documentSession
                    .Query<User, Users_ByProvider>()
                    .Where(x => x.ProviderUserId == admin.ProviderUserId && x.Provider == admin.Provider)
                    .FirstOrDefault();

                if (exisitingAdmin == null)
                {
                    admin.AddRoles(_roles);
                    documentSession.Store(admin);
                }
                else
                {
                    exisitingAdmin.AddRoles(_roles);
                }
            }

            documentSession.SaveChanges();
        }

        private void AddModerators(IDocumentSession documentSession)
        {
            foreach (var moderator in _configurationManager.GetModerators())
            {
                var exisitingModerator = documentSession
                    .Query<User, Users_ByProvider>()
                    .Where(x => x.ProviderUserId == moderator.ProviderUserId && x.Provider == moderator.Provider)
                    .FirstOrDefault();

                if (exisitingModerator == null)
                {
                    moderator.AddRoles(_roles.Where(x => x.Id == "roles/moderator" || x.Id == "roles/user"));
                    documentSession.Store(moderator);
                }
                else
                {
                    exisitingModerator.AddRoles(_roles.Where(x => x.Id == "roles/moderator" || x.Id == "roles/user"));
                }
            }

            documentSession.SaveChanges();
        }
    }
}