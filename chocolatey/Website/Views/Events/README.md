# Events Area

## Summary

This document is intended to instruct a user how to add a new event to the Events area.

<!-- TOC depthFrom:2 -->
- [Step 1: Copy Example Files](#step-1-copy-example-files)
- [Step 2: Change The File Names](#step-2-change-the-file-names)
  - [Webinar Event](#if-this-is-a-webinar-event)
  - [Workshop Event](#if-this-is-a-workshop-event)
  - [Conference Event](#if-this-is-a-conference-event)
- [Step 3: Update The Markdown File](#step-3-update-the-markdown-file)
- [Step 4: Update The CSHTML Files](#step-4-update-the-cshtml-file)
- [Step 5: Review And Ensure Files Are Added To Project](#step-5-review-and-ensure-files-are-added-to-project)
<!-- /TOC -->

## Step 1: Copy Example Files

1) Navigate to the folder `chocolatey\Website\Views\Events` and copy/paste the .md file `01-00-ExampleEvent.md`
1) Navigate to the folder `chocolatey\Website\Views\Events\Files` and copy/paste the .cshtml file `ExampleEvent.cshmtl

## Step 2: Change the File Names    

The file names follow a common pattern to help organize them and associate the type of event and their corresponding thumbnail images.

### If this is a **Webinar** event:

* **Markdown File**
  * Start the `.md` file name with `01`.
  * The second number should be next available number in the list. 
    * For example, let say there is a file already named `01-08-AwesomeWebinar.md`, the number of this new webinar should be `09`
  * To finish, the ending of the file name should be the webinar title with no spaces, and capital letters for each word.

Example `.md` file: `01-05-AwesomeWebinar.md`

* **CSHTML File**
  * Name this file the webinar title with no spaces, and capital letters for each word.
  * This should match the ending of the `.md` file.

Example `.cshmtl` file: `AwesomeWebinar.cshtml`

### If this is a **Workshop** event:

* **Markdown File**
  * Start the `.md` file name with `02`.
  * The second number should be next available number in the list. 
    * For example, let say there is a file already named `02-08-AwesomeWorkshop.md`, the number of this new workshop should be `09`
  * To finish, the ending of the file name should be the workshop title with no spaces, and capital letters for each word.

Example `.md` file: `02-04-AwesomeWorkshop.md`

* **CSHTML File**
  * Name this file the workshop title with no spaces, and capital letters for each word.
  * This should match the ending of the `.md` file.

Example `.cshmtl` file: `AwesomeWorkshop.cshtml`

### If this is a **Conference** event:

* **Markdown File**
  * Start the `.md` file name with `03`.
  * The second number should be next available number in the list. 
    * For example, let say there is a file already named `03-08-AwesomeConference.md`, the number of this new conference should be `09`
  * To finish, the ending of the file name should be the conference title with no spaces, and capital letters for each word.

Example `.md` file: `03-07-AwesomeConference.md`

* **CSHTML File**
  * Name this file the conference title with no spaces, and capital letters for each word.
  * This should match the ending of the `.md` file.

Example `.cshmtl` file: `AwesomeConference.cshtml`

## Step 3: Update The Markdown File

* **`IsArchived`** - Change this to `false`
*  For all other fields, see the table below.

| Name              | Required | Options/Type									| Default/Example					| Description					|
|-------------------|----------|------------------------------------------------|-----------------------------------|-------------------------------|
| IsArchived        | true     | true or false								| false                             | If this field is set to `true`, then it will not appear in the list of events, and the event page will be redirected back to the main events area. |
| URL               | true     | string											| example-event	                    | This will be the title of the event, all lowercase, with dashes in between each word. |
| Type              | true     | webinar, workshop, conference					| webinar                           | |
| EventDate         | true     | yyyyMMddTHH:mm:ss								| 20250101T15:00:00                 | This date is primarily used to configure the countdown clock via JS, and to move items on the main event page from "upcoming" to "on-demand". The format uses a 24-hour clock from 0 to 23 and should be set to UTC. |
| Time              | true     | string											| 10-11 AM CDT (8-9 AM PDT / 3-4 PM GMT) | This is the user friendly time of the event. This format can be whatever makes sense for the event. |
| Duration          | true     | string											| 1 hour | The estimated length of event. |
| Title             | true     | string with every word capitalized				| Example Event                     | |
| Tagline           | true     | string with every word capitalized				| Awesome Example Tagline Here      | This tagline will appear near the bottom of the page as a closing call-to-action. |
| Speakers          | false    | string seperated by commas						| John Doe, Jane Doe                | If no speakers are defined yet, use `TBD`. |
| Image             | true     | html											| `<img class="lazy img-fluid" src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7" data-src="/content/images/events/00.jpg" alt="Example Event" title="Example Event" />` | This image will appear on the event listing, and aspect ratio of 16:9. Name the file similar to the `.md` file, but exclude title. Remember to update attributes! |
| RegisterLink		| true     | string											| https://chocolatey.zoom.us/webinar/register/WN_1U91f_LiR8uLiQXJtVwlKg	| This is the link the user will sign up for the event at. |	
| Tags			    | true     | string seperated by commas - **Year required** | 2020, tag1, tag2                  | The year is required at a minimum. Tags are not utilized curretnly, but may be in the futre. Choose tags wisely. |
| Summary           | true     | 2-3 sentences									| Here is a short sentence describing what this event is. This should be strait to the point. | This summary is what will show up on the home event page when the event is listed. |
| Post              | true     | markdown										|                                   | Markdown is encouraged in this section, but html can also be used. This is the bulk of the content that will appear on the webinar page. This is defined by anything below the `---` in the `.md` file. |

## Step 4: Update The CSHTML File

Most of the content in the `.cshtml` will be pulled from the `.md` file, but because of the flexibility of this page, some items still need to be manually updated. Further enhancements may template out additional parts of the `.cshmtl` file.

* **Search for `Example Event`** - Do a page search for `Example Event` and replace with the **exact title** of the event you specified in the `.md` file.
* **`var description`** - This description is what shows up when the event is shared via social media and is also used for website SEO.
* **Header Area** - Skim through this section to find any hard coded default content. It should be quite obvious what should need to be updated. There should not be a need to change any of the html structure or classes.
* **Speaker Area**
  * **Speaker Bio** - Give each speaker a short bio, or remove the entire `p` element if they do not have a bio.
  * **Speaker Image** 
    * Update the speaker image src (or leave the default if the speaker does not have one), putting it in the same folder as the default image is located. The file name should follow the structure `firstname-lastname.jpg`. It should be no larger than 400x400 and no smaller than 100x100.
    * Be sure to update any `alt` & `title` attributes with the speakers name
  * **Speaker Name and Titles** - Update the hardcoded `Jane Doe` or ``John Doe` to the actual name and their positions.
  * **Social Media Links** - Insert the actual links to the speakers Twitter and LinkedIn profiles. If they do not want to share these, simple delete this element from the `.cshtml`.
  * **Adding/Removing Number of Speakers** - Speakers can be added or removed from this section. Simply find the element `<div class="event-speaker"></div>` and either copy or delete everything in between.

## Step 5: Review And Ensure Files Are Added To Project

1) Go to your solution explorer and click to "Show All Files" in the top tools area (in the solution explorer).
1) Find the new files that you have added and right click on them. Choose "Include in Project".
1) In the top navigation of Visual Studio, press the Green play button, which will build the website and add these new files to `Website.csproj`.
1) Review all changes. When everything is happy, proceed with committing and submit PR as outlined [here.](https://github.com/chocolatey/choco/blob/master/CONTRIBUTING.md#prepare-commits)