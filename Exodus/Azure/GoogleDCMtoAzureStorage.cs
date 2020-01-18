//// © 2018 Soverance Studios
//// Scott McCutchen
//// DevOps Engineer
//// scott.mccutchen@soverance.com

//// a haiku about working at DRUM Agency
////
//// " I do dev ops stuff "
//// " DRUM pays less than job is worth "
//// " I stop dev ops stuff "

//// This class leverages asynchronous I/O in order to download a DCM/DFA report through Google API, and upload it into an Azure Storage Blob.

//// In order to use this class, you must install the following prerequisites:
////
//// - Visual Studio 2017 15.3 or later
//// - Microsoft .NET Framework 4.6.1 or later - https://www.microsoft.com/net/download/windows
//// - Microsoft Azure Storage Client Library - https://www.nuget.org/packages/WindowsAzure.Storage/
//// - Microsoft Azure Storage Data Movement Library - https://www.nuget.org/packages/Microsoft.Azure.Storage.DataMovement/
//// - Google API .NET Library - https://developers.google.com/api-client-library/dotnet/get_started
//// - Google OAuth2 Library - https://developers.google.com/identity/protocols/OAuth2
//// - Google DCM/DFA Reporting and Trafficking API for .NET Library version 3.0 - https://developers.google.com/doubleclick-advertisers/getting_started

//// Google API Service Account Credential References:
//// - You must have created a service account within your Google API project that has "Domain-Wide Delegation" enabled for it
//// - You can then use this API service account to impersonate other accounts within the Google-managed domain
//// - see here for more info:  https://developers.google.com/api-client-library/php/auth/service-accounts#delegatingauthority
//// - This service account must be granted API access to various services in our Google-managed domain by using the Google Admin portal.
//// - Within the Google Admin console, go to "Security -> Advanced -> Manage API Client Access"
//// - The client ID for this app is 111845787581380415754
//// - The client ID must be granted the following API scopes:
//// - https://www.googleapis.com/auth/dfareporting 

//// Documentation References: 
//// - What is a Storage Account - https://docs.microsoft.com/azure/storage/common/storage-create-storage-account
//// - Getting Started with Blobs - https://docs.microsoft.com/azure/storage/blobs/storage-dotnet-how-to-use-blobs
//// - Blob Service Concepts - https://docs.microsoft.com/rest/api/storageservices/Blob-Service-Concepts
//// - Blob Service REST API - https://docs.microsoft.com/rest/api/storageservices/Blob-Service-REST-API
//// - Blob Service C# API - https://docs.microsoft.com/dotnet/api/overview/azure/storage?view=azure-dotnet
//// - Scalability and performance targets - https://docs.microsoft.com/azure/storage/common/storage-scalability-targets
////   Azure Storage Performance and Scalability checklist https://docs.microsoft.com/azure/storage/common/storage-performance-checklist
//// - Storage Emulator - https://docs.microsoft.com/azure/storage/common/storage-use-emulator
//// - Asynchronous Programming with Async and Await  - http://msdn.microsoft.com/library/hh191443.aspx
//// - Azure Storage Data Movement Library - https://azure.microsoft.com/en-us/blog/introducing-azure-storage-data-movement-library-preview-2/


/////////////////////////////////////////////////////////////////////////////////////////////////
////////
////////   Start Azure Async Controller
////////
/////////////////////////////////////////////////////////////////////////////////////////////////

//// NOTE:  This class originally was copied over from a different .NET MVC project that I worked on for a client.  
//// Since they did not end up using it, I wanted to keep the code around in case I need it in the future.  It's now open-source for everyone.
//// it has never fully been implemented within Exodus, which is why this entire class is commented out
//// to make it operational within Exodus will require additional considerations that I am not yet ready to make, for a feature I will not currently use

//// This class also had an MVC Controller class associated with it, which I will reproduce here.  
//// NOTE:  Within Exodus, the controller class would obviously need to be moved out of this file to make it operational

//namespace Exodus.Controllers
//{
//    /// <summary>
//    /// The Asynchronous Data Movement Controller class provides access to data movement management between Google DCM/DFA and Microsoft Azure Storage.    
//    /// <br />
//    /// <br />
//    /// Some things to know about the asynchronous transfer process within this application:
//    ///      <ul>
//    ///          <li>Asynchronous downloads from Google DCM/DFA report servers occur in 10 MB chunks.</li>
//    ///          <li>For asynchronous uploads to Azure Storage, the source block blob will be retrieved using 4 MB chunks, with each chunk copied to the destination blob individually.</li>
//    ///          <li>The source block blob will be locked during the upload process to prevent changes</li>
//    ///          <li>Copy Blob operations are automatically retried upon any intermittent failures such as network failures, server busy etc.</li>
//    ///      </ul>
//    /// </summary>
//    [Authorize]
//    public class AsyncUploaderController : ApiController
//    {
//        /// <summary>
//        /// Asynchronously upload the specified DCM/DFA report to Azure Storage.
//        /// </summary>
//        /// <param name="emailToImpersonate">The DCM email address you wish to impersonate.  This must be an email address managed by Drum Agency.</param>
//        /// /// <param name="profileID">The DCM Profile ID.</param>
//        /// /// <param name="reportID">The DCM Report ID.</param>
//        /// /// <param name="bDirectCopy">Whether or not you wish to perform a direct copy of the data from DCM to Azure.  Setting this to false will download the report to the application server before uploading it to Azure Storage.  DIRECT COPY FEATURE DOES NOT YET WORK - YOU MUST SET THIS TO FALSE!!</param>
//        [HttpPost]
//        [ResponseType(typeof(AsyncUploaderController))]
//        public IEnumerable<string> Post(string emailToImpersonate, string profileID, string reportID, bool bDirectCopy)
//        {
//            return new string[] { Exodus.Azure.GoogleDCMtoAzureStorage.InitializeAsyncUpload(emailToImpersonate, profileID, reportID, bDirectCopy) };
//        }
//    }
//}

/////////////////////////////////////////////////////////////////////////////////////////////////
////////
////////   Start Azure Async Uploader
////////
/////////////////////////////////////////////////////////////////////////////////////////////////

//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Blob;
//using Microsoft.WindowsAzure.Storage.DataMovement;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Json;
//using Google.Apis.Download;
//using Google.Apis.Dfareporting.v3_0;
//using Google.Apis.Dfareporting.v3_0.Data;

//namespace Exodus.Azure
//{
//    public static class AsyncUploader
//    {
//        // Define the Azure Storage connection string
//        // THIS METHOD IS INSECURE!
//        // To be more secure, we could instead store the connection string as an environment variable on the machine that will run this application.
//        // we could then use Environment.GetEnvironmentVariable("storageconnectionstring"); to obtain the string in a more secure manner
//        // This connection string is for the 'unifiedstorage' storage account within the UA-Domain resource group
//        // You can find this connection string in the Azure Portal, by viewing the storage account's "Access Keys" blade
//        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=unifiedstorage;AccountKey=EXIuwXKpEdNLIFFj6KF5uZMtZPsY/Sfk7jiWPPrsNmciLsYHLXwTmn9l2vOfIAj+I+UGc/7CljSFIekJg4pm/A==;EndpointSuffix=core.windows.net";

//        // This is the return string used by the primary HttpGet method from the AsyncController
//        static string getOutput = "";

//        // configure progress monitoring
//        //public delegate void SuccessEvent();  // configure a delegate that can be used to point to functions
//        //public static event SuccessEvent SucceededEvent;  // establish an event that can be subscribed to by objects

//        // This event is called when the upload finally succeeds
//        public static void HandleUploadSucceeded()
//        {
//            Console.WriteLine(System.Environment.NewLine + "Upload has been completed successfully.");
//            Console.WriteLine("Press 'Enter' to exit the Exodus Data Services Asynchronous Uploader application.");
//            //Toolkit.ToolkitEventLog.WriteEntry("Upload has been completed successfully.", EventLogEntryType.Information, 999);
//        }

//        // configure a storage array for the OAuth 2.0 scopes we will request
//        private static readonly IEnumerable<string> OAuthScopes = new[] {
//            DfareportingService.Scope.Dfareporting
//        };

//        // Get the Google API service account credentials from the JSON file (GoogleApiKeys.json)
//        private static ServiceAccountCredential GetServiceAccountCredentials(String pathToJsonFile, String emailToImpersonate)
//        {
//            // Load and deserialize credential parameters from the specified JSON file.
//            JsonCredentialParameters parameters;
//            using (Stream json = new FileStream(pathToJsonFile, FileMode.Open, FileAccess.Read))
//            {
//                parameters = NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(json);
//            }

//            // Create a credential initializer with the correct scopes.
//            ServiceAccountCredential.Initializer initializer =
//                new ServiceAccountCredential.Initializer(parameters.ClientEmail)
//                {
//                    Scopes = OAuthScopes
//                };

//            // Configure impersonation (if applicable).
//            // This does nothing if the string is empty
//            if (!String.IsNullOrEmpty(emailToImpersonate))
//            {
//                initializer.User = emailToImpersonate;
//            }

//            // Create a service account credential object using the deserialized private key.
//            ServiceAccountCredential credential =
//                new ServiceAccountCredential(initializer.FromPrivateKey(parameters.PrivateKey));

//            return credential;
//        }

//        public static string InitializeAsyncUpload(string emailToImpersonate, string profileID, string reportID, bool bDirectCopy)
//        {
//            //getOutput = "Impersonating " + emailToImpersonate + " with Profile ID " + profileID + " to obtain Report ID " + reportID + ".  ";

//            // Connect to Google API DCM / DFA Reporting Services
//            return ConnectToReportingServices(null, emailToImpersonate, profileID, reportID, bDirectCopy);
//        }

//        // Connect to Google API and DCM/DFA Reporting Services
//        public static string ConnectToReportingServices(DfareportingService service, string emailToImpersonate, string profileID, string reportID, bool bDirectCopy)
//        {
//            // This file is obtained from the Google API portal, in the "Credentials -> Service Account Keys" section
//            // You must download that file from the portal, and copy it's contents into the GoogleApiKeys.json file located in this project
//            // Note that the GoogleApiKeys.json file has been configured to copy to the application's build output directory (THIS METHOD IS INSECURE!)
//            // This could be secured by removing the build copy, and distributing the .json file separately from the application via Windows AD Group Policy 
//            // (distributing the keys separately would limit application usage to specified domain users)
//            // You would then replace this string with a full, direct local path to the .json file
//            // We use the fully-qualified MapPath function to find this file within the context of ASP.NET
//            string pathToJsonFile = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/GoogleApiKeys.json");

//            // The "emailToImpersonate" variable allows the user of this application to specify a DCM account to impersonate
//            // This is only applicable to service accounts which have enabled domain-wide delegation
//            // Setting this field will not allow you to impersonate a user from a domain you don't own (e.g., gmail.com).

//            // Build service account credential.
//            ServiceAccountCredential credential =
//                GetServiceAccountCredentials(pathToJsonFile, emailToImpersonate);

//            // validate the credentials
//            if (credential != null)
//            {
//                //getOutput += "Authentication for " + credential.Id.ToString() + " was successful.  ";
//            }
//            else
//            {
//                getOutput = "Failed to obtain service credentials.  ";
//                return getOutput;
//            }

//            // Create a Dfareporting service object.
//            // Note: application name should be replaced with a value that identifies your application within the Google API portal.
//            service = new DfareportingService(
//                new BaseClientService.Initializer
//                {
//                    HttpClientInitializer = credential,
//                    ApplicationName = "DrumToolkit"
//                }
//            );

//            // this is just a simple debug check to make sure the reporting service was successfully initialized
//            if (service != null)
//            {
//                //getOutput += "DFA reporting service was successfully initialized.  ";
//            }

//            // Start the Google profile collection
//            return GetGoogleProfile(service, credential, emailToImpersonate, profileID, reportID, bDirectCopy);
//        }

//        // Prompts the user for the Google profile with which to interact with DCM/DFA Reporting Services
//        public static string GetGoogleProfile(DfareportingService service, ServiceAccountCredential credential, string emailToImpersonate, string profileID, string reportID, bool bDirectCopy)
//        {
//            // try to retrieve all profiles based on the user-provided values
//            try
//            {
//                // Retrieve and print all user profiles for the current authorized user.
//                UserProfileList profiles = service.UserProfiles.List().Execute();

//                // if we found one or more profiles, print them in the console window
//                if (profiles.Items.Count > 0)
//                {
//                    //getOutput += "Available Profile IDs for " + emailToImpersonate + " include: ";

//                    foreach (UserProfile profile in profiles.Items)
//                    {
//                        //getOutput += profile.ProfileId + ", ";
//                    }
//                }
//                // error output if we find no profiles
//                else
//                {
//                    getOutput += "The specified user, " + emailToImpersonate + ", is not associated with any DCM profiles.  ";
//                    return getOutput;
//                }

//                bool bValidProfileInput = long.TryParse(profileID, out long specifiedProfileId);  // collect the user input and store it as long type
//                bool bProfileExistence = false;  // we'll use this momentarily...

//                // if user input profile ID is invalid...
//                // this is basically checking whether you entered any letters into the ID - numbers only!
//                if (!bValidProfileInput)
//                {
//                    getOutput += "The specified Profile ID must be numerical.  ";
//                    return getOutput;
//                }
//                // if the profile ID entered was valid, continue
//                else
//                {
//                    // we should definitely have more than one profile if the code made it this far...
//                    foreach (UserProfile profile in profiles.Items)
//                    {
//                        // here we're just checking that the user made a valid input of the profile ID, and we'll error out otherwise.
//                        if (profile.ProfileId == specifiedProfileId)
//                        {
//                            bProfileExistence = true;  // if the above query becomes true just once, set existence to true
//                        }
//                    }

//                    // if user did not specify a valid profile ID, restart this function
//                    if (!bProfileExistence)
//                    {
//                        getOutput += "The specified Profile ID was not found.  ";
//                        return getOutput;
//                    }
//                    // if the profile ID specified was valid, continue
//                    else
//                    {
//                        // Get the specified report
//                        return GetGoogleReport(service, credential, Convert.ToInt64(profileID), Convert.ToInt64(reportID), bDirectCopy);
//                    }
//                }
//            }
//            // any exceptions thrown during this process will be printed to the console, and the app restarted
//            catch (Exception ex)
//            {
//                string error = "Error returned from the service: " + ex.Message;
//                getOutput += error;
//                return getOutput;
//            }
//        }

//        // Get the Google API report
//        public static string GetGoogleReport(DfareportingService service, ServiceAccountCredential credential, long specifiedProfileId, long specifiedReportId, bool bDirectCopy)
//        {
//            string reportName = null;  // we'll use this later...
//            ReportList reports = null;
//            ReportsResource.ListRequest reportRequest = service.Reports.List(specifiedProfileId);  // Get the complete report list for the specified Profile ID
//            reports = reportRequest.Execute();  // execute the report request and store the results

//            //getOutput += System.Environment.NewLine + reports.Items.Count.ToString() + " reports were found for the Profile ID: " + specifiedProfileId;
//            //getOutput += System.Environment.NewLine + "Found reports are displayed below, with the most recent report first." + System.Environment.NewLine;

//            // sort the report array in descending order by the last modified time attribute
//            IList<Report> list = reports.Items;  // store the reports into a new array we can more easily enumerate
//            IEnumerable<Report> sortedEnum = list.OrderByDescending(a => a.LastModifiedTime);  // sort the list in descending order by last modified time
//            IList<Report> sortedList = sortedEnum.ToList();  // add the new sorted list to a new IList array

//            // loop through the sorted reports and show some basic info
//            //foreach (Report report in sortedList)
//            //{
//            //    getOutput += report.Id + ", ";
//            //}

//            bool bReportExistence = false;  // we'll use this later...

//            // loop through the reports and verify that the user input is a valid report
//            foreach (Report report in sortedList)
//            {
//                if (report.Id == specifiedReportId)
//                {
//                    bReportExistence = true;  // report exists!
//                    reportName = report.Name;  // store the report name so we can pass it to the download functions
//                }
//            }

//            // if user-defined report was found... continue
//            if (bReportExistence)
//            {
//                // get the files associated with this report
//                return GetGoogleReportFiles(service, credential, specifiedProfileId, specifiedReportId, reportName, bDirectCopy);
//            }
//            // if the user entered a valid numerical sequence as the Report ID, but it was not found...
//            else
//            {
//                getOutput += "The specified Report ID was not found.";
//                return getOutput;
//            }
//        }

//        // Get the Google Report Files
//        public static string GetGoogleReportFiles(DfareportingService service, ServiceAccountCredential credential, long specifiedProfileId, long specifiedReportId, string reportName, bool bDirectCopy)
//        {
//            FileList reportFiles;

//            // Get all of the files associated with the specified report
//            ReportsResource.FilesResource.ListRequest filerequest = service.Reports.Files.List(specifiedProfileId, specifiedReportId);
//            reportFiles = filerequest.Execute();

//            // proceed if one or more files were found to be associated with the report ID
//            if (reportFiles.Items.Count > 0)
//            {
//                Console.WriteLine(System.Environment.NewLine);  // readability...

//                // The 'File' declaration is ambiguous between Google's API and System.IO
//                // Why the fuck Google would do this, I do not know...
//                // However, we can easily circumvent this issue by specifying the fully-qualified variable type

//                // sort the file list in descending order, so that the most recent file appears first
//                IList<Google.Apis.Dfareporting.v3_0.Data.File> list = reportFiles.Items;  // store the files in a new array that we can more easily enumerate
//                IEnumerable<Google.Apis.Dfareporting.v3_0.Data.File> sortedEnum = list.OrderByDescending(a => a.LastModifiedTime);  // sort the list in descending order by last modified time
//                IList<Google.Apis.Dfareporting.v3_0.Data.File> sortedList = sortedEnum.ToList();  // add the new sorted list to a new IList array

//                Google.Apis.Dfareporting.v3_0.Data.File reportFile = sortedList[0];  // store a reference to the most recent file

//                if (reportFile.Status == "REPORT_AVAILABLE")
//                {
//                    //getOutput += "STATUS:  " + reportFile.Status + ".  ";

//                    // if the HttpGet request was made with bDirectCopy as True, we'll do the direct async copy process.  Ohterwise, do the regular download/upload process
//                    if (bDirectCopy)
//                    {
//                        // Process the Asynchronous Copy 
//                        //ProcessAsyncCopy(credential, reportFile).GetAwaiter().GetResult();
//                    }
//                    else
//                    {
//                        // download the report locally
//                        return DownloadReport(service, credential, specifiedProfileId, specifiedReportId, reportFile, reportName);                        
//                    }
//                }
//                else
//                {
//                    getOutput += "The most recent file associated with this Report ID returned status: " + reportFile.Status.ToString() + ".  ";
//                    return getOutput;
//                }
//            }
//            else
//            {
//                getOutput += "The specified Report ID has no files associated with it.  ";
//                return getOutput;
//            }
//            return getOutput;
//        }

//        // Generate a file name for the DFA report if one is not specified, which seems to be fucking never.
//        private static string GenerateFileName(Google.Apis.Dfareporting.v3_0.Data.File file, string reportName)
//        {
//            // If no filename is specified, use the file ID instead.
//            string fileName = file.FileName;
//            if (String.IsNullOrEmpty(fileName))
//            {
//                // strip the timestamp from the end of the report name, and then use it to set the file name                
//                string rawName = reportName;
//                fileName = rawName.Remove(rawName.Length - 16);
//                Console.WriteLine(System.Environment.NewLine + "Downloading {0}", fileName);
//            }

//            String extension = "CSV".Equals(file.Format) ? ".csv" : ".xml";

//            return fileName + extension;
//        }

//        // simple progres update
//        private static void DownloadProgressUpdate(IDownloadProgress progress)
//        {
//            Console.WriteLine("STATUS: " + progress.Status + " - Bytes Downloaded: " + progress.BytesDownloaded);

//            if (progress.Status == DownloadStatus.Completed)
//            {
//                Console.WriteLine(System.Environment.NewLine + "DOWNLOAD COMPLETE");
//            }
//        }

//        // Downloads the specified DFA report file via Google API
//        public static string DownloadReport(DfareportingService service, ServiceAccountCredential credential, long specifiedProfileId, long specifiedReportId, Google.Apis.Dfareporting.v3_0.Data.File reportFile, string reportName)
//        {
//            // configure the get request
//            FilesResource.GetRequest getRequest = service.Files.Get(specifiedReportId, reportFile.Id.Value);
//            getRequest.MediaDownloader.ChunkSize = 10;  // adjust the chunk size (in MB) used when downloading the file
//            getRequest.MediaDownloader.ProgressChanged += DownloadProgressUpdate;  // configure progress output
//            System.IO.FileStream outFile;
//            //outFile.Close();  // close the file after creating it so it can be reopened for writing
//            //System.IO.Directory.CreateDirectory("~/Reports/");  // create the Reports directory if it does not exist

//            using (outFile = new System.IO.FileStream(System.Web.HttpContext.Current.Server.MapPath("~/DCM/Reports/" + GenerateFileName(reportFile, reportName)), System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
//            {
//                //await getRequest.DownloadAsync(outFile);
//                getRequest.Download(outFile);
//                //getOutput += "File " + reportName + " downloaded to " + outFile.Name + ".  ";
//            }

//            // Process the Asynchronous Upload
//            return ProcessAsyncUpload(outFile.Name, reportFile, reportName);
//        }

//        // Gets the current context (progress) of the data transfer - Currently only used for Async Copy... but could be modified to be used for Async Upload as well
//        public static SingleTransferContext GetSingleTransferContext(TransferCheckpoint checkpoint, Google.Apis.Dfareporting.v3_0.Data.File reportFile)
//        {
//            // store the file's total size in bytes so we can display it during the upload process
//            long totalTransferLength = reportFile.Format.Length;
//            int percentComplete = 0;

//            SingleTransferContext context = new SingleTransferContext(checkpoint)
//            {
//                ProgressHandler = new Progress<TransferStatus>((progress) =>
//                {
//                    percentComplete = (int)(progress.BytesTransferred * 100 / totalTransferLength);  // get current % complete
//                    string StatusDescription = string.Format("Progress: " + percentComplete.ToString() + "%  -  " + progress.BytesTransferred + " bytes transferred of " + totalTransferLength + " total bytes.");
//                    Console.WriteLine(StatusDescription);
//                })
//            };

//            return context;
//        }

//        // sets the permissions on an Azure Blob container to be publicly accessible
//        public static void SetPublicContainerPermissions(CloudBlobContainer container)
//        {
//            BlobContainerPermissions permissions = container.GetPermissionsAsync().GetAwaiter().GetResult();
//            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
//            container.SetPermissionsAsync(permissions).GetAwaiter().GetResult();
//        }

//        // This function will asynchronously upload a specified local file
//        public static string ProcessAsyncUpload(string filePath, Google.Apis.Dfareporting.v3_0.Data.File reportFile, string reportName)
//        {
//            // define some other variables we'll use later
//            CloudBlobContainer cloudBlobContainer = null;
//            string sourceFile = Path.GetFullPath(filePath);  // full local file path + extension
//            string sourceFileExtension = Path.GetExtension(filePath);  // get just the extension of this file to determine it's type
//            Task uploadTask = null;
//           // SucceededEvent += new SuccessEvent(HandleUploadSucceeded);  // subscribe to the success event

//            try
//            {
//                // the storageConnectionString should always be available, since we're setting it directly.
//                // if we were obtaining the string dynamically, we'd want to have some error checking.
//                // so this check is irrelevant, but maintained for posterity.
//                if (storageConnectionString == null)
//                {
//                    // if the storageConnectionString is null, print an error
//                    getOutput +=
//                        "A connection string has not been defined in the system environment variables. " +
//                        "Add a environment variable name 'storageconnectionstring' with the actual storage " +
//                        "connection string as a value.  ";
//                    return getOutput;
//                }

//                // Create a reference to the Azure Cloud Storage account
//                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

//                // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
//                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

//                // Create a reference to the 'drum-data' container where we'll store our uploads
//                // You can obviously specify whatever container you want here, including a container that does not yet exist
//                cloudBlobContainer = cloudBlobClient.GetContainerReference("drum-data");
//                // Create the container if it does not exist
//                cloudBlobContainer.CreateIfNotExistsAsync().GetAwaiter().GetResult();
//                // set the container to be publicly accessible
//                SetPublicContainerPermissions(cloudBlobContainer);

//                // Get a reference to the location where the blob is going to go.                
//                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(GenerateFileName(reportFile, reportName));
//                // set the content type of the file so that it is uploaded correctly to blob storage.
//                // failure to set the content type of the file will result in the upload blob being extensionless
//                cloudBlockBlob.Properties.ContentType = sourceFileExtension;

//                // check if the blob already exists in the cloud.  If it does not, process the async upload.
//                // This code was updated in a later release of the Azure Storage Library to do this check asynchronously.
//                if (!cloudBlockBlob.ExistsAsync().GetAwaiter().GetResult())
//                {
//                    // Asynchronously upload the file to Azure Storage Blob.
//                    // For more information on asynchronously copying blobs, see here:  https://blogs.msdn.microsoft.com/windowsazurestorage/2012/06/12/introducing-asynchronous-cross-account-copy-blob/
//                    // The source block blob will be locked during the upload process to prevent changes - any modification to the source file will cause the upload process to fail
//                    // The source block blob will be retrieved using 4 MB chunks, with each chunk copied to the destination blob individually.
//                    // Copy Blob operations are automatically retried upon any intermittent failures such as network failures, server busy etc. 
//                    // Any failures are recorded in x-ms-copy-status-description which would let users know why the copy is still pending.
//                    // You can query more information about the blob while it's pending upload using the Get Blob Properties operation
//                    // see here for more info:  https://docs.microsoft.com/en-us/rest/api/storageservices/Get-Blob-Properties

//                    // Use the interfaces from the new Azure Storage Data Movement library to upload the blob

//                    // Setup the number of concurrent operations
//                    // this setting determines how many files you can upload simultaneously
//                    // we're really only focused on transferring an individual file right now, but you can do a lot more concurrently on all cpu threads
//                    TransferManager.Configurations.ParallelOperations = 64;

//                    // store the file's total size in bytes so we can display it during the upload process
//                    long totalTransferLength = new System.IO.FileInfo(sourceFile).Length;
//                    TransferCheckpoint checkpoint = null;  // a checkpoint object, used to pick up where we left off when the user pauses the upload
//                    SingleTransferContext context = GetSingleTransferContext(checkpoint, reportFile);  // a new transfer context object

//                    // Begin the asynchronous upload process
//                    uploadTask = TransferManager.UploadAsync(sourceFile, cloudBlockBlob, null, context, CancellationToken.None);
//                    getOutput += "Asynchronous upload to Azure has begun.  This may take a while depending on the file size.  Once complete, the report can be found at: " + cloudBlockBlob.Uri;
//                }
//                // if the blob already exists in the cloud container, then prompt the user to exit the application.  No upload is necessary.
//                else
//                {
//                    getOutput += "The specified blob already exists in the Azure container at:  " + cloudBlockBlob.Uri;
//                    return getOutput;
//                }
//            }
//            // catch and print any errors to the console
//            catch (StorageException ex)
//            {
//                getOutput += ex.Message;
//                return getOutput;
//            }

//            return getOutput;
//        }

//        // This function will asynchronously upload a specified remote file
//        public static async Task<string> ProcessAsyncCopy(ServiceAccountCredential credential, Google.Apis.Dfareporting.v3_0.Data.File reportFile)
//        {
//            // define some other variables we'll use later
//            CloudBlobContainer cloudBlobContainer = null;

//            // subscribe to the success event
//            //SucceededEvent += new SuccessEvent(HandleUploadSucceeded);

//            try
//            {
//                // the storageConnectionString should always be available, since we're setting it directly.
//                // if we were obtaining the string dynamically, we'd want to have some error checking.
//                // so this check is irrelevant, but maintained for posterity.
//                if (storageConnectionString == null)
//                {
//                    // if the storageConnectionString is null, print an error
//                    getOutput +=
//                        "A connection string has not been defined in the system environment variables. " +
//                        "Add a environment variable name 'storageconnectionstring' with the actual storage " +
//                        "connection string as a value.";
//                }

//                // Create a reference to the Azure Cloud Storage account
//                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

//                // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
//                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

//                // Create a reference to the 'drum-data' container where we'll store our uploads
//                // You can obviously specify whatever container you want here, including a container that does not yet exist
//                cloudBlobContainer = cloudBlobClient.GetContainerReference("drum-data");
//                // Create the container if it does not exist
//                await cloudBlobContainer.CreateIfNotExistsAsync();
//                // set the container to be publicly accessible
//                SetPublicContainerPermissions(cloudBlobContainer);

//                // Get a reference to the location where the blob is going to go.                
//                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(reportFile.Id.ToString());

//                // check if the blob already exists in the cloud.  If it does not, process the async upload.
//                // This code was updated in a later release of the Azure Storage Library to do this check asynchronously.
//                if (!await cloudBlockBlob.ExistsAsync())
//                {
//                    // Asynchronously upload the file to Azure Storage Blob.
//                    // For more information on asynchronously copying blobs, see here:  https://blogs.msdn.microsoft.com/windowsazurestorage/2012/06/12/introducing-asynchronous-cross-account-copy-blob/
//                    // The source block blob will be locked during the upload process to prevent changes - any modification to the source file will cause the upload process to fail
//                    // The source block blob will be retrieved using 4 MB chunks, with each chunk copied to the destination blob individually.
//                    // Copy Blob operations are automatically retried upon any intermittent failures such as network failures, server busy etc. 
//                    // Any failures are recorded in x-ms-copy-status-description which would let users know why the copy is still pending.
//                    // You can query more information about the blob while it's pending upload using the Get Blob Properties operation
//                    // see here for more info:  https://docs.microsoft.com/en-us/rest/api/storageservices/Get-Blob-Properties

//                    // Use the interfaces from the new Azure Storage Data Movement library to upload the blob

//                    // Setup the number of concurrent operations
//                    // this setting determines how many files you can upload simultaneously
//                    // we're really only focused on transferring an individual file right now, but you can do a lot more concurrently on all cpu threads
//                    TransferManager.Configurations.ParallelOperations = 64;

//                    // Configure the variables we'll use for the upload process
//                    string ApiUrl = reportFile.Urls.ApiUrl;
//                    string AuthToken = "Bearer" + " " + credential.Token;
//                    Uri RemoteSource = new Uri(ApiUrl);
//                    TransferCheckpoint checkpoint = null;  // a checkpoint object, used to pick up where we left off when the user pauses the upload
//                    SingleTransferContext context = GetSingleTransferContext(checkpoint, reportFile);  // a new transfer context object
//                    CancellationTokenSource cancellationSource = new CancellationTokenSource();  // a new cancellation token, in case the user wants to resume the upload
//                    Task task;
//                    ConsoleKeyInfo keyinfo;

//                    //getOutput += "Transfer started...";
//                    //Console.WriteLine("Press 'C' to temporarily suspend the transfer..." + System.Environment.NewLine);

//                    try
//                    {
//                        // Setup the number of concurrent operations
//                        // this setting determines how many files you can upload simultaneously
//                        // we're really only focused on transferring an individual file right now, but you can do a lot more concurrently on all cpu threads
//                        TransferManager.Configurations.ParallelOperations = 64;

//                        // configure the copy task
//                        // The CopyAsync task is designed to copy data from other cloud services (copying directly to the source is far more efficient than downloading then uploading)

//                        task = TransferManager.CopyAsync(RemoteSource, cloudBlockBlob, true, null, context, cancellationSource.Token);

//                        // while the task is processing, watch for user input on the 'C' key
//                        while (!task.IsCompleted)
//                        {
//                            if (Console.KeyAvailable)
//                            {
//                                keyinfo = Console.ReadKey(true);
//                                // if the user presses the 'C' key during the upload
//                                if (keyinfo.Key == ConsoleKey.C)
//                                {
//                                    cancellationSource.Cancel();  // cancel the upload process, which effectively pauses the 
//                                }
//                            }
//                        }

//                        if (cancellationSource.IsCancellationRequested)
//                        {
//                            getOutput += "Transfer will resume in 3 seconds...";
//                            Thread.Sleep(3000);
//                            checkpoint = context.LastCheckpoint;
//                            context = GetSingleTransferContext(checkpoint, reportFile);
//                            getOutput += "Resuming transfer...";
//                            await TransferManager.CopyAsync(RemoteSource, cloudBlockBlob, true, null, context, cancellationSource.Token);
//                        }

//                        await task;
//                        getOutput += "Asynchronous Copy has begun.  Check your Azure Storage container momentarily.";
//                        return getOutput;
//                    }
//                    catch (Exception ex)
//                    {
//                        getOutput += "An error occured: " + ex.Message;
//                        return getOutput;
//                    }
//                }
//                // if the blob already exists in the cloud container, then prompt the user to exit the application.  No upload is necessary.
//                else
//                {
//                    getOutput += "The specified blob already exists in the cloud container. No upload necessary.";
//                    return getOutput;
//                }
//            }
//            // catch and print any errors to the console
//            catch (StorageException ex)
//            {
//                getOutput += "An error was returned from the service: " + ex.Message;
//                return getOutput;
//            }
//            finally
//            {
//                Console.ReadLine();  // force the console window to stay open while the upload process completes                
//            }
//        }
//    }
//}
