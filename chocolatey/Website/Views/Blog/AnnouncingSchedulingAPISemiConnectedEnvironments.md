Url: announcing-scheduling-api-semi-connected-environments
Title: Announcing Central Management Deployments Scheduling, API and Support for Semi-Connected Environments
Published: 20201116
Author: Chocolatey Team
Tags: deployments, news, press-release, chocolatey for business
Keywords: news, press-release, chocolatey for business, chocolatey, deploy packages, schedule packages, api, devops, software deployment automation, software management automation
Summary: We are excited to release the latest version of Chocolatey Central Management Deployments which includes Deployment Scheduling, support for Semi-Connect Environments and our brand new API!
Image: <img src="/content/images/blog/centralmanagement.png" alt="Chocolatey Central Management" title="Chocolatey Central Management" />
---

With the paint not even dry on the new release of Chocolatey Central Management, we are excited to tell you all about it. We have a lot of new features to share that we know you have been waiting for!

### Register For Our Webinar
To showcase our new features there is no better way than to see it working! We’ll be demoing all the great features in a webinar on December 3rd, 2020. [Register](https://chocolatey.zoom.us/webinar/register/2016050271071/WN__AtcpeIbQnGACco6PE2QbA) to reserve your place now.

If you can’t make it, don’t worry. The webinar will be recorded and be available to watch soon after the presentation using the same [registration link](https://chocolatey.zoom.us/webinar/register/2016050271071/WN__AtcpeIbQnGACco6PE2QbA).

### What is Chocolatey Central Management Deployments
If you’re not familiar with Chocolatey Central Management Deployments then our previous blog post announcing its release will help you understand what Deployments are, and how you can use them to easily manage your Windows endpoints. You can also find more information on our documentation pages.

### Start Central Management Deployments On A Schedule Or During a Maintenance Window
This is an exciting new feature to help you schedule deployments for a specific time period such as during a maintenance window, allowing you to manage your endpoints in line with your current procedures or workflows.

You can add a schedule to any deployment by selecting a start time. If you need to use an end time, for a maintenance window for example, you can add that as well!

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_Schedule.gif" alt="Edit Deployment Schedule" title="Edit Deployment Schedule" /></div>

Find out more about [working with Central Management Deployments](https://chocolatey.org/docs/central-management-deployments).

### Support Semi-Connected Environments with Central Management Deployments
With organizations allowing staff to work from home, we’ve been hearing from customers who want to use Central Management Deployments in environments where the endpoints are not always online.

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_MachineContactTimeout.jpg" alt="Machine Contact Timeout" title="Machine Contact Timeout" /></div>

To allow you to deploy to these environments, Central Management Deployments now allows you to set a Machine Contact Timeout to wait for a machine to be connected to the network before it is given a deployment step (and even make it wait indefinitely for those machines to check in).

Find out more about [working with Central Management Deployments in a Semi-Connected Environment](https://chocolatey.org/docs/central-management-deployments#how-can-i-run-deployments-in-a-semi-connected-environment).

### Automate Central Management with the new API
<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_API.gif" alt="CCM API" title="CCM API" /></div>

To cover the Central Management API in depth we will have a blog post available soon! If you can’t wait, [join our webinar on December 3rd](https://chocolatey.zoom.us/webinar/register/2016050271071/WN__AtcpeIbQnGACco6PE2QbA) where we will be also show you how to create a recurring deployment using the API!

To make it even easier to work with the Central Managements API, we are releasing a PowerShell module called ChocoCCM that is now available in the [PowerShell Gallery](https://www.powershellgallery.com/packages/ChocoCCM) which you can install with:

```powershell
Install-Module -Name ChocoCCM
```

The ChocoCCM module also provides functions to work with:

* Roles
* Groups
* Computers
* Deployments
* Outdated Software
* Reports

### Chocolatey Quick Deploy Environment Includes All These Great Features!
Chocolatey Quick Deploy Environment (QDE) is a unified architecture that includes:

* Sonatype Nexus Repository OSS as your package repository.
* Jenkins as your automation engine, letting you run tasks both on-demand and on a schedule and also helps you auto-internalize packages from the Chocolatey Community Repository using included PowerShell scripts.
* Chocolatey Central Management. No introduction is necessary!
* Internal Deployment scripts to help you configure the solution.

QDE now includes the latest release of Central Management, so you can try Scheduling, API automation and work in a semi-connected environment using one solution straight away.

To get QDE into your Chocolatey For Business environment, please [reach out to us](https://chocolatey.org/contact/quick-deployment) so we can work to get you set up.

### We’re Not Finished...
This is the sixth release of Central Management and our second release of Deployments. We have a lot more to come in future releases, all driven by the needs and pain points of customers.

We're also enhancing the documentation for CCM, including supporting materials and videos, that you’ll soon be able to access.

### When Can I Get Deployments?
If you are a current customer, please see the documentation for upgrade - you can grab the latest release now! You can also reach out to your Chocolatey Software representative for more details. If you are not a current customer, you can also [reach out to our team](https://chocolatey.org/contact/trial) to get started. We are excited to get it in your hands!

And remember, to see all of these great new features in action [register for our webinar](https://chocolatey.zoom.us/webinar/register/2016050271071/WN__AtcpeIbQnGACco6PE2QbA) on December 3rd, 2020.