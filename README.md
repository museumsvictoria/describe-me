#### IMPORTANT
Project has now been closed down as it achieved its goal of describing the bulk of Museums Victorias collection images.

## About Describe Me 
[Describe Me](http://describeme.museumvictoria.com.au/) is a website that asks you to write alternative text (alt-text) for images contained within Museums Victoria’s collection in order to enhance our data making it more useful for people who are blind or have low vision.

## What Technology does it use?
* ASP.Net Framework 4.5 (MVC4).
* RavenDb v2.0 (build 2360) for persistence.
* SignalR v0.52 for client messaging.
* IMu Client for connecting to EMu (Museum Collections Management System).
* Ninject for IoC.
* HTML5, LESS, JQuery front end.
* Application pattern based on Simple CQRS by Greg Young.

## How do I use it?
The Describe Me source code has been provided for those curious in how it has been put together as there is a dependency on Museums Victoria’s particular flavour of Collection Management which supplies the data that runs the site.  Please feel free to look at and re-use any parts of the code but don’t expect a self-contained project.

**Developer:** Michael Mason
**Designer:** Simone Downey (nomuu)
