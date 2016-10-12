using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using ImageResizer;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Catalog;
using System.IO;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Net;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class AmazonS3PictureService : PictureService
    {
        #region Fields
        private static readonly object s_lock = new object();
        private readonly ISettingService _settingService;

        private readonly NopConfig _config;

        private readonly IRepository<Picture> _pictureRepository;
        private readonly IDataProvider _dataProvider;
        private readonly IRepository<ProductPicture> _productPictureRepository;

        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IDbContext _dbContext;
        public readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private IProductService _productService;

        #endregion

        #region Ctor

        public AmazonS3PictureService(IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            ILogger logger,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IDataProvider dataProvider,
            NopConfig config,IProductService productService)
            : base(pictureRepository,
                productPictureRepository,
                settingService,
                webHelper,
                logger,
                dbContext,
                eventPublisher,
                mediaSettings,dataProvider)
        {
            this._pictureRepository = pictureRepository;
            this._productPictureRepository = productPictureRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._logger = logger;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._dataProvider = dataProvider;
            this._mediaSettings = mediaSettings;
            this._config = config;
            this._productService = productService;
        }

        public override byte[] LoadPictureBinary(Picture picture)
        {
            string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string fileName = string.Format("{0}_0.{1}", picture.Id.ToString("0000000"), lastPart);




            // var filePath = GetPictureLocalPath(fileName);

            try
            {

                System.Net.WebResponse objResponse;
                try
                {
                    objResponse = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["AmazonS3URL"].ToString() + "/0/" + picture.Id + "/" + fileName).GetResponse();

                    byte[] m_Bytes = ReadToEnd(objResponse.GetResponseStream());
                    objResponse.Close();
                    return m_Bytes;
                    
                }
                catch(Exception ex)
                {
                   
                    objResponse = WebRequest.Create(System.Configuration.ConfigurationSettings.AppSettings["AmazonS3URL"].ToString()  + "/" + picture.Id + "/" + fileName).GetResponse();
                    byte[] m_Bytes = ReadToEnd(objResponse.GetResponseStream());
                    objResponse.Close();
                    return m_Bytes;


                }


            }
            catch (Exception)
            {
                return null;
            }
        }

        private static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public override string GetPictureUrl(int pictureId, int targetSize = 0, bool showDefaultPicture = true, string storeLocation = null, PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = GetPictureById(pictureId);
            return GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }




        public override string GetPictureUrl(Picture picture, int targetSize = 0, bool showDefaultPicture = true, string storeLocation = null, PictureType defaultPictureType = PictureType.Entity)
        {
            string url = string.Empty;
            byte[] pictureBinary = null;
            if (picture != null)
                pictureBinary = LoadPictureBinary(picture);
            if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
            {
                if (showDefaultPicture)
                {
                    url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                }
                return url;
            }

            string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
            string thumbFileName;
            if (picture.IsNew)
            {
                DeletePictureThumbs(picture);

                //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                picture = UpdatePicture(picture.Id,
                    pictureBinary,
                    picture.MimeType,
                    picture.SeoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    false,
                    false);
            }
            lock (s_lock)
            {
                string seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure
                if (targetSize == 0)
                {
                    //original size (no resizing required)
                    thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                        string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart) :
                        string.Format("{0}.{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName, picture.Id);
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        SaveThumb(thumbFilePath, thumbFileName, pictureBinary, picture.Id);
                    }
                }
                else
                {
                    //resizing required
                    thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                        string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart) :
                        string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName, picture.Id);
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName))
                    {
                        using (var stream = new MemoryStream(pictureBinary))
                        {
                            Bitmap b = null;
                            try
                            {
                                //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                b = new Bitmap(stream);
                            }
                            catch (ArgumentException exc)
                            {
                                //_logger.Error(string.Format("Error generating picture thumb. ID={0}", picture.Id), exc);
                            }
                            if (b == null)
                            {
                                //bitmap could not be loaded for some reasons
                                return url;
                            }

                            using (var destStream = new MemoryStream())
                            {
                                var newSize = CalculateDimensions(b.Size, targetSize);
                                ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                                {
                                    Width = newSize.Width,
                                    Height = newSize.Height,
                                    Scale = ScaleMode.Both,
                                    Quality = 90
                                });
                                var destBinary = destStream.ToArray();
                                SaveThumb(thumbFilePath, thumbFileName, destBinary, picture.Id);
                                b.Dispose();
                            }
                        }
                    }
                }
            }
            url = GetThumbUrl(thumbFileName, storeLocation, picture.Id);
            return url;
        }

        protected override void SavePictureInFile(int pictureId, byte[] pictureBinary, string mimeType)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string filename = "";

            
            //filename = "product/" + oProductPicture.ProductId + "/" +  string.Format("{0}_0.{1}", pictureId.ToString("0000000"), lastPart);

       
            filename =  pictureId + "/" + string.Format("{0}_0.{1}", pictureId.ToString("0000000"), lastPart);

            //Upload to AWS
            UploadToAmazon(pictureBinary, filename);

        }
        private void UploadToAmazon(byte[] pictureBinary, string fileName)
        {
            try
            {
                //foldername = Path.Combine(_webHelper.MapPath("~/content/images/"), foldername);

                //NameValueCollection appSettings = ConfigurationManager.AppSettings;
                string awsAccessKeyId = _config.AmazonS3AccessKey;
                string awsSecretAccessKey = _config.AmazonS3SecretKey;
                TransferUtility transferUtility = new TransferUtility(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSoutheast1);

                // DirectoryInfo directoryInfo = new DirectoryInfo(foldername);
                TransferUtilityUploadRequest transferUtilityUploadRequest = new TransferUtilityUploadRequest();
                transferUtilityUploadRequest.BucketName = "image.eldago.com";
                transferUtilityUploadRequest.CannedACL = S3CannedACL.PublicRead;
                //transferUtilityUploadRequest.Timeout = 360000000;
                transferUtilityUploadRequest.PartSize = 2097152L;
                transferUtilityUploadRequest.AutoCloseStream = true;
                Stream stream = new MemoryStream(pictureBinary);
                //string key = fileInfo.FullName.ToLower().Replace(_config.TempImageFolder.Trim().ToLower(), "").Replace("\\", "/");
                transferUtilityUploadRequest.Key = fileName;
                transferUtilityUploadRequest.InputStream = stream;
                //System.Threading.Tasks.Task oTask = transferUtility.UploadAsync(transferUtilityUploadRequest);
                AsyncCallback callback = new AsyncCallback(this.uploadComplete);
                IAsyncResult asyncResult = transferUtility.BeginUpload(transferUtilityUploadRequest, callback, fileName);
                transferUtility.EndUpload(asyncResult);

            }
            catch (Exception ex)
            {

            }
        }

        private void uploadComplete(IAsyncResult result)
        {

        }
        public bool Exists(string file)
        {
            bool result;
            try
            {
                string key = file.ToLower().Replace(_config.TempImageFolder.Trim().ToLower(), "").Replace("\\", "/");

                string awsAccessKeyId = _config.AmazonS3AccessKey;
                string awsSecretAccessKey = _config.AmazonS3SecretKey;
                AmazonS3Client amazonS3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSoutheast1);
                amazonS3Client.GetObjectMetadata(new GetObjectMetadataRequest() { BucketName = "image.eldago.com", Key = key });
                result = true;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
            return result;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override void DeletePictureThumbs(Picture picture)
        {


          

            //NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string awsAccessKeyId = _config.AmazonS3AccessKey;
            string awsSecretAccessKey = _config.AmazonS3SecretKey;



            AmazonS3Client amazonS3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSoutheast1);
            ListObjectsRequest oRequest = new ListObjectsRequest();
            oRequest.BucketName = "image.eldago.com";
            oRequest.Prefix =  picture.Id.ToString();
            ListObjectsResponse oListresponse =  amazonS3Client.ListObjects(oRequest);
            List<S3Object> arrObject =  oListresponse.S3Objects.ToList();
            DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest();
            deleteObjectRequest.BucketName = "image.eldago.com";
            foreach (S3Object oFile in arrObject)
            {
                
                deleteObjectRequest.Key = oFile.Key;
                try
                {
                    DeleteObjectResponse oResponse = amazonS3Client.DeleteObject(deleteObjectRequest);

                }
                catch (Exception ex)
                {
                    //this.LogError("Error in Deleting files from Amazon " + filename + " " + ex.ToString());
                }
            }


            //Delete Folder

            deleteObjectRequest.Key =  picture.Id.ToString();
            try
            {
                DeleteObjectResponse oResponse = amazonS3Client.DeleteObject(deleteObjectRequest);

            }
            catch (Exception ex)
            {
                //this.LogError("Error in Deleting files from Amazon " + filename + " " + ex.ToString());
            }








            //int pictureid = picture.Id;


            //string awsAccessKeyId = _config.AmazonS3AccessKey;
            //string awsSecretAccessKey = _config.AmazonS3SecretKey;
            //AmazonS3Client amazonS3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSoutheast1);
            //DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest();
            //deleteObjectRequest.BucketName = "eldago.indotrading.com";
            //deleteObjectRequest.Key = filename.ToLower().Replace(Helper.GetConfigValue("ImageFolder").Trim().ToLower(), "").Replace("\\", "/");
            //deleteObjectRequest.Key = "/" + pictureid.ToString();
            //try
            //{
            //    DeleteObjectRequest deleteFolderRequest = new DeleteObjectRequest();
            //    deleteFolderRequest.BucketName = "eldago.indotrading.com";
            //    String delimiter = "/";
            //    deleteFolderRequest.Key = string.Concat(pictureid.ToString(), delimiter);
            //    DeleteObjectResponse deleteObjectResponse = amazonS3Client.DeleteObject(deleteFolderRequest);


            //}
            //catch (Exception ex)
            //{
            //this.LogError("Error in Deleting files from Amazon " + filename + " " + ex.ToString());

            //}








        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        public virtual string GetThumbLocalPath(string thumbFileName, int pictureId)
        {

            var thumbFilePath = System.Configuration.ConfigurationSettings.AppSettings["AmazonS3URL"] + "/" + pictureId + "/" + thumbFileName;
            return thumbFilePath;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        public virtual string GetThumbUrl(string thumbFileName, string storeLocation = null, int pictureid = 0)
        {
           

            var url = System.Configuration.ConfigurationSettings.AppSettings["AmazonS3URL"];


            url = url  + "/" + pictureid + "/" + thumbFileName;
            return url;
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            try
            {
                WebResponse objResponse;
                objResponse = WebRequest.Create(thumbFilePath).GetResponse();
                if (objResponse.ContentLength > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="binary">Picture binary</param>
        public virtual string SaveThumb(string thumbFilePath, string thumbFileName, byte[] binary, int pictureid = 0)
        {
            
          
            UploadToAmazon(binary, pictureid + "/" + thumbFileName);

            return null;

        }

        #endregion
    }
}
