# CMS AB Testing  & Optimizely Full Stack Integration

This version of CMS AB Testing connects to Optimizely Full Stack and creates A/B Testing experimentation in Full Stack. When a conversion is made, Metrics is also updated in Full Stack. Results of Metrics is displayed from Full Stack into AB Test view within CMS.

## Start AB Test in CMS
Test is created in Full Stack with Experimentation. Event with page title is created to track Metrics.

## User Lands on AB Test Page in CMS
When user lands on AB Test page of CMS, Page Tracking event is called passing user context. User is assigned either variation A (ON) or variation B (OFF) version of the test page. 

* Variation “On” maps to New Draft version of AB Test Page.
* Variation “Off” maps to Current Published version of AB Test Page.

## User navigates to conversion page in CMS
When same user navigates to conversion page from AB Test page, Experimentation decision is made and is logged in Metrics in experimentation results. Same Metrics is visible from within CMS.

## Abort AB Test
When Test is aborted, it is deleted from CMS and Experimentation is switched off in Optimizely Full Stack.

## Publish Winner
When winner is selected, selected version of page is published in CMS and experimentation is switched off in Optimizely Full Stack.
 
## Installation Steps
Required Packages in CMS content cloud for Optimizely Full Stack Integration to Work
* Research.Marketing.Experimentation (Version 1.0.0)
* RestSharp (Version 108.0.1)
Configuration Changes
Add below configuration changes in AppSettings.json in root in your Content Cloud Project

```
"full-stack": {
    "ProjectId": "21972070188",
    "APIVersion": 1,
    "EnviromentKey": "production",
    "SDKKey": "",
    "CacheInMinutes": 20,
    "RestAuthToken": "",
    "EventName": "page_view",
    "EventDescription": "Event to calculate page view metrics"
  }
```
## Session Cookie Required
Unique Session Cookie with name “FullStackUserGUID” is required for Full Stack User Context. Please create Unique session cookie with this name if does not exists.
Below is code to create unique session cookie in Foundation.

```
string userId = _cookieService.Get("FullStackUserGUID");
if (string.IsNullOrEmpty(userId))
	_cookieService.Set("FullStackUserGUID", Guid.NewGuid().ToString());
```

## Fields required for testing 

* Test Goal
* Page View and Conversion Page
* Participation Percentage

![Screenshot1](images/Screenshot1.png?raw=true "Screenshot1")


## Startup Class Changes Required in Content Cloud 

```
services.AddABTesting(_configuration.GetConnectionString("EPiServerDB"));
services.Configure<FullStackSettings>(_configuration.GetSection("full-stack"));
```
 
## Experimentation Results
Experimentation Results is shown in CMS AB Test view as below

![Screenshot2](images/Screenshot2.png?raw=true "Screenshot2")

