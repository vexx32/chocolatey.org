Url: announcing-deployments
Title: Announcing Central Management Deployments - Manage Simple or Complex Scenarios with Ease
Published: 20200521
Author: Chocolatey Team
Tags: deployments, news, press-release, chocolatey for business
Keywords: news, press-release, chocolatey for business, chocolatey, deploy packages, devops, software deployment automation, software management automation
Summary: Deployments will allow you to manage your Windows endpoint machines simply, efficiently, and securely. Your simple and complex deployment scenarios are all covered by Central Management Deployments.
Image: <img src="/content/images/blog/centralmanagement.png" alt="Chocolatey Central Management" title="Chocolatey Central Management" />
---

<div class="text-center"><img class="img-fluid border mb-3 w-60" src="/content/images/blog/centralmanagement.png" alt="Chocolatey Central Mangement" title="Chocolatey Central Mangement" /></div>

We are gearing up for the 3rd release of Chocolatey Central Management (CCM), and this one will include an amazing feature that many of you may have been waiting for - managing endpoints with a full Chocolatey solution! We are really excited to share with you what Central Management Deployments will offer and how you can get your hands on it!

<!-- TOC depthFrom:2 -->

- [Why Did We Create Central Management Deployments?](#why-did-we-create-central-management-deployments)
- [What is Central Management Deployments?](#what-is-central-management-deployments)
- [What Does Central Management Deployments Look Like?](#what-does-central-management-deployments-look-like)
  - [Top Level Deployments Screen](#top-level-deployments-screen)
  - [Creating And Editing A Deployment](#creating-and-editing-a-deployment)
  - [Reporting](#reporting)
- [More to Come](#more-to-come)
- [When Can I Get Deployments?](#when-can-i-get-deployments)

<!-- /TOC -->

### Why Did We Create Central Management Deployments?
We often hear from Windows System Engineers that they are looking for a simple and easy way to manage Windows endpoints that offers advanced functionality when they need it. As Chocolatey Software has been a leader in Windows automation for over nine years, it was a natural progression to offer a simple and intuitive solution for managing remote Windows endpoints, especially given Chocolatey’s guiding principle of making hard concepts approachable.

What prompted Chocolatey Software to create a solution for managing endpoints, and why was it the number one feature request?

In working with System Engineers, we found folks are looking for a solutions that is:

* Simple - Simple to learn and use
* Secure - Modern security practices demand modern approaches
* Efficient - Should not wait hours and hours on the system
* Advanced - Able to handle advanced scenarios
* Conventions - Handles defaults for most scenarios, but overridable
* Multiple Step Targets - Able to work with multiple steps across possibly different computer endpoints at each step
* Low Maintenance - should not require heavy architecture and should be easy to maintain
* Scalable - Able to easily scale when necessary
* Capable of Managing All Software - Not all software comes w/installers, but all software can have security findings.

There are existing solutions out there that for one reason or another have fallen short of expectations:

* Some require specialized training
* Some are overkill for organizational needs
* Simpler solutions don't fully support more advanced scenarios when they are needed
* Even when those solutions are implemented correctly, they still cannot manage all Windows software without Chocolatey

To draw on the last point a bit more, almost every endpoint management solution out there only reports software inventory based on what is listed in Programs and Features. We've found that software listed in Programs and Features only accounts for about 50-80% of the software that is being managed on Windows machines. With Chocolatey, you can manage 100% of the software you need to deploy. Security conscious organizations need to be able to report on and manage 100% of things that can have security findings.

### What is Central Management Deployments?

<div class="text-center"><img class="img-fluid border mb-3 w-100" src="/content/images/blog/CCM_Deployments_Edit_UpdateOrder.gif" alt="View Deployment - Steps" title="View Deployment - Steps" /></div>

We are excited to add Deployments to Chocolatey Central Management (CCM). This will enable teams to securely manage endpoints w/PowerShell scripts and states of Chocolatey packages directly. The CCM web interface helps with easy set-up, management, and reporting on deployments to endpoints. This will be part of the Chocolatey for Business (C4B) offering and current customers will be able to take advantage of it when released.

Central Management (including Deployments) provides a fantastic set of features that give organizations complete software management. Organizations using configuration management (like Puppet, Ansible, Chef, etc) will find that Central Management is complementary to what they provide. While configuration managers do really well with ensuring state and correcting configuration drift, when it comes to orchestration (especially cross machine), configuration management is quite limited. CCM Deployments provides IT teams the ability to easily orchestrate simple or complex scenarios in a fraction of the time over traditional approaches.

With the initial release of CCM Deployments, you will find the following functionality:

* Create target groups to deploy to
* Create a deployment with one or more steps
* Each step can target multiple groups, and different groups in each step if desired
* Script a Chocolatey package
* With additional permissions, run a full PowerShell script instead
* Choose how failures in each step are handled
* Reorder steps
* Control permissions on who can deploy Chocolatey packages and who can run full scripts
* See progress on active deployments
* View logs for computers that executed a deployment step
* Report on completed deployments including exporting to PDF for sharing with executive staff

With multiple steps being able to target different groups, Deployments really enables you to manage complex scenarios when you need to. As an example if you have a website with a database backend, you may need to take the site offline first and upgrade the database before you upgrade the website. This is easily achieved with CCM Deployments.

### What Does Central Management Deployments Look Like?
Let's go through a quick preview.

#### Top Level Deployments Screen
Coming into the the Deployments section of the site, you are greeted with different sections.

<div class="text-center"><img class="img-fluid border mb-3 w-75" src="/content/images/blog/CCM_Deployments_TopLevel_SM-lg.png" alt="View Deployments - Main Screen" title="View Deployments - Main Screen" /></div>

The sections are:

* Drafts - these are deployments that are still being created and are not ready to be used
* Ready - these are deployments that are ready to go, they are just waiting to be started
* Active - these are deployments that are currently executing
* Completed - these are deployments that have finished, with whatever status they might have ended up in

#### Creating And Editing A Deployment
If we have a deployment that is in draft/ready status, we can look over the aspects of it:

<div class="text-center"><img class="img-fluid border mb-3 w-100" src="/content/images/blog/CCM_Deployments_Edit_View.gif" alt="View Deployment - Expand Steps" title="View Deployment - Expand Steps" /></div>

If we want to edit any of the steps for a deployment that has not been activated, we bring up that step and take a look at the different sections. The script side has basic and advanced types. This is basic:

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_StepModal_BasicCommand_SM-lg.png" alt="Edit Deployment Step - Basic Script" title="Edit Deployment Step - Basic Script" /></div>

Note the advanced items (which are not expanded by default):

* Execution timeout in seconds - how long to let the command run before timing out? Defaults to `4 hours`.
* Valid exit codes - what exit codes indicate success? Defaults to `0, 1605, 1614, 1641, 3010`.
* Machine contact timeout in minutes - how long to attempt contact with a computer before timing out. This is set at `20 minutes` in the first release. In a future release we'll allow you to configure this, but this is set by default in this release.
* Fail overall deployment if not successful - Should the deployment status show failed if this step is not successful? Defaults to `true`.
* Only run other deployment steps if successful - Coupled with the previous item, should the deployment stop if this step is not successful on every computer? Or should it execute other steps? Defaults to `false`.

If you have additional privileges, you can make/edit advanced scripts:

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_StepModal_AdvancedCommand_SM-lg.png" alt="Edit Deployment Step - Advanced Script" title="Edit Deployment Step - Advanced Script" /></div>

Note there is a section with script tips that is not expanded in the image above (`Show script tips`). Some folks are experts at writing PowerShell and other folks may need a refresher from time to time.

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_StepModal_AdvancedCommand_Tips_SM-lg.png" alt="Edit Deployment Step - Advanced Script Tips" title="Edit Deployment Step - Advanced Script Tips" /></div>

Once we've decided on our script, we need to target groups/collections of computers:

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Edit_StepModal_Groups_SM-lg.png" alt="Edit Deployment Step - Select Target Groups" title="Edit Deployment Step - Select Target Groups" /></div>

#### Reporting
When we move a deployment to active, it will begin to run and we can start seeing the details of the deployment as it progresses:

<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Reports_DeploymentDetails_SM-lg.png" alt="View Deployment Report - Deployment Overall Results" title="View Deployment Report - Deployment Overall Results" /></div>

We can drill into the details of a particular step:
<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Reports_StepDetails_SM-lg.png" alt="View Deployment Report - Deployment Step Results" title="View Deployment Report - Deployment Step Results" /></div>

Note that the log comes back for each computer in a step. If there is an error we can open up the log and go right to each error the system detects:
<div class="text-center"><img class="img-fluid border mb-3" src="/content/images/blog/CCM_Deployments_Reports_ErrorLog_SM-lg.png" alt="View Deployment Report - Step Computer Log Details" title="View Deployment Report - Step Computer Log Details" /></div>

### More to Come
As you can see, the initial release of Central Management Deployments has a lot to offer! Deployments serves as the foundation for many other things we want to light up with Chocolatey Central Management (CCM), and we have some future enhancements we'll be adding to Deployments itself as we move into future release phases for CCM.

We're also enhancing the documentation for CCM, including supporting materials and videos, and hope to share that with you soon!

After everything releases, we'll rev the version of [Quick Deployment Environment (QDE)](https://chocolatey.org/docs/quick-deployment-environment) to include CCM w/Deployments so folks will have another way of getting up and running quickly.

### When Can I Get Deployments?
The timeline for release is early June, with betas going out to select customers soon. Reach out to your Chocolatey Software representative for more details. If you are not a current customer, you can also [reach out to our team](https://chocolatey.org/contact/trial) to get started. We are excited to get it in your hands!
