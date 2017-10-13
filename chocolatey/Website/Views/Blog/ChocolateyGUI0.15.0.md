Url: chocolatey-gui-0.15.0-released
Title: Chocolatey GUI 0.15.0 has just been released!
Author: Gary Ewan Park
Published: 20171014
Tags: chocolatey oss, chocolatey gui, news, press-release
Keywords: chocolatey oss, chocolatey gui, news, press-release, chocolatey, automate existing systems, dev ops, software deployment automation, software management automation
Summary: Chocolatey GUI 0.15.0, the graphical user interface for Chocolatey, has just been released.  This is a major release of the application which brings with it a number of improvements and features, as well as fixing a number of bugs.
---

We are very happy to announce the release of version 0.15.0 of Chocolatey GUI!  This is a major overhaul of the application including a huge number of bug fixes, as well as a number of features and improvements.  This release has been a while in the making, but we hope that it will be worth the wait!

The following will highlight a number of the major changes that are included in this release, and you can also see a full set of [release notes](https://github.com/chocolatey/ChocolateyGUI/releases/tag/0.15.0).

## Chocolatey Lib

In previous versions of Chocolatey GUI, we shelled out directly to PowerShell to invoke the various commands that are available within Chocolatey.  In this release, we are now using the official [chocolatey.lib](https://www.nuget.org/packages/chocolatey.lib), which means we now have a much better integration with the features that Chocolatey offers.  In doing this refactoring, we have also eliminated a number of the bugs that were present in the previous release.

## New Branding

When we switched to using WPF as the base framework for Chocolatey GUI, we introduced what we thought was a sensible colour scheme, emphasing on the "chocolatey" theme.  Looking back, we think we went a little too far.  In the 0.15.0 release, we have changed the branding again, which is more in-keeping with the overall Chocolatey branding.

If you are interested in seeing the evolution of the branding for this release, you can follow along in this [commit](https://github.com/chocolatey/ChocolateyGUI/commit/ed894cc4e16abe5d33de1275efae49803d8d1919) and then this [commit](https://github.com/chocolatey/ChocolateyGUI/commit/c7a92e35a4b7e43cc611d7f8fd854ba8bb3171e2#diff-bcb0247f0d40fae191239e76914070a0).

We would love to hear any feedback that you might have related to the new branding.

## Localization of Chocolatey GUI

We know that Chocolatey GUI is used across the world, by speakers of lots of different languages.  With this in mind, the decision was taken to support translation of all the major parts of Chocolatey GUI.  This will be an ongoing effort, however, in this release, there is now support for English, Norwegian, German and Swedish.

If you are interested in helping support your language in Chocolatey GUI, then please reach out via an issue on the GitHub repository.

We are going to start using the amazing [Transifex](https://www.transifex.com/) service to better support this effort going forward.

## Release Notes

To find out information about all the features, improvements and bugs that were included in this release, have a look at the [release notes](https://github.com/chocolatey/ChocolateyGUI/releases/tag/0.15.0).

## Contributors

This release would not have been possible without the help of the amazing Chocolatey Community!  We thank you all for your support!

The people who helped out with this release are:

- [RichiCoder1](https://github.com/RichiCoder1)
- [pascalberger](https://github.com/pascalberger)
- [gep13](https://github.com/gep13)
- [mwallner](https://github.com/mwallner)
- [AdmiringWorm](https://github.com/AdmiringWorm)
- [magol](https://github.com/magol)
- [cniweb](https://github.com/cniweb)
- [ferventcoder](https://github.com/ferventcoder)
- Magnus Österlund
- [punker76](https://github.com/punker76)

### Learn More

* Check out the [documentation](https://chocolatey.github.io/ChocolateyGUI/about).
* Learn about other features available in [Chocolatey for Business](https://chocolatey.org/compare).
* [Contact us](https://chocolatey.org/contact) to find out more and setup your evaluation of Chocolatey for Business today.