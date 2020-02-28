using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Soap;
//using System.Runtime.Serialization.Formatters.Binary;

namespace ImageSharp.TestGui
{
    [Serializable]
    public class DirectoryDescriptor
    {

        [JsonPropertyName("GalleryName")]
        public string GalleryName { get => directoryName; }

        [JsonPropertyName("DirectoryName")]
        public string DirectoryName { get => "/" + directoryName; }
        protected string directoryName;
        
        [JsonPropertyName("FullDirectoryPath")]
        public string FullPath { get => "/" + fullPath.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/"); }
        private string fullPath;
          
        [JsonPropertyName("GalleryIndexHTMLFile")]
        public string GalleryIndexHTMLFile { get => "/" + indexfilename.FullName.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/").Replace(".md",".html"); }
        
        [JsonPropertyName("GalleryIndexMarkdownFile")]
        public string IndexFileName { get => "/" + indexfilename.FullName.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/"); }
        protected System.IO.FileInfo indexfilename;
        
        [JsonPropertyName("ImageThumbnail")]
        public string ImageThumbnail { get; set; }
        
        [JsonPropertyName("PhotoGalleries")] //JsonExtensionData
        public List<DirectoryDescriptor> Directories { get => directories; set => directories = value; }
        private System.Collections.Generic.List<DirectoryDescriptor> directories = new System.Collections.Generic.List<DirectoryDescriptor>();
        
        [JsonPropertyName("Images")]
        public List<ImageDescriptor> Images { get => images; set => images = value; }
        private System.Collections.Generic.List<ImageDescriptor> images = new System.Collections.Generic.List<ImageDescriptor>();

        // Ensures ever run of this has some changes
        [JsonPropertyName("GeneratedDateTime")]
        public string GeneratedDateTime { get => System.DateTime.UtcNow.ToString(); }
        
        private DirectoryInfo GitHubRepoRoot;

        public DirectoryDescriptor(string directoryname, string fullpath)
        {
            this.directoryName = directoryname;
            this.fullPath = fullpath;
        }

        public void FixUpPaths(DirectoryInfo di)
        {
            GitHubRepoRoot = di;
            foreach(DirectoryDescriptor dd in directories)
            { 
                dd.FixUpPaths(di);
            }
            bool first = true;
            foreach(ImageDescriptor i in images)
            {
                i.FixUpPaths(di);
                if(first || i.Name.ToLower().StartsWith("thumbnail"))
                {
                    first = false; // we need to have a thumbnail, so it's the first image
                    ImageThumbnail = i.JSON_ThumbnailFile;
                }
            }
        }
        
        // Only 1 directory deep support currently. not a problem because Jekyll doesn't support it
        public void SaveMDFiles(string Domain, System.IO.DirectoryInfo ImagesDirectory)
        {
            if(directoryName == ImagesDirectory.Name)
            {
                indexfilename = new FileInfo(ImagesDirectory.FullName + "\\Index.md");
            }else{
                indexfilename = new FileInfo(ImagesDirectory.FullName + "\\" + directoryName + ".md");
            }
            
            foreach(DirectoryDescriptor dd in directories)
            {
                dd.SaveMDFiles(Domain, ImagesDirectory);
            }
            //we need to overwrite them even if they exist (but we need some fortmat to allow edits)
            {
                CreateIndexFile(indexfilename, Domain, ImagesDirectory);
                System.Console.WriteLine("Create : " + indexfilename.FullName);
            }
        }


        public void CreateIndexFile(System.IO.FileInfo fi, string Domain, System.IO.DirectoryInfo ImagesDirectory)
        {
            System.IO.TextWriter tw = fi.CreateText();
            bool DirectoryListing = false;
            
            tw.WriteLine("---");
            if(directoryName == ImagesDirectory.Name)
            {
                tw.WriteLine("permalink: /" + ImagesDirectory.Name + "/index.html");
                tw.WriteLine("title: /" + ImagesDirectory.Name + "/index" );
                tw.WriteLine("page_variable: " +  this.directoryName  );
                DirectoryListing = true;
            }else{               
                tw.WriteLine("permalink: /" + ImagesDirectory.Name + "/" + this.directoryName + ".html");
                tw.WriteLine("title: /" + ImagesDirectory.Name + "/" +  this.directoryName );
                tw.WriteLine("page_variable: " +  ImagesDirectory.Name + "/" +  this.directoryName );
            }         
            
            tw.WriteLine("Gallery_Page: " +  this.directoryName );
            tw.WriteLine("Gallery_Name: " +  this.directoryName );
            tw.WriteLine("---");
            
            tw.WriteLine();
            tw.WriteLine();
            tw.WriteLine("{% include header.md %}");
            tw.WriteLine();
            
            
                
            tw.WriteLine("# " + this.directoryName);
            tw.WriteLine();
            tw.WriteLine("----");
            tw.WriteLine("<div class=\"image-container-" + this.directoryName + " ImgContainer\">");
            foreach(ImageDescriptor i in images)
            {
                WriteImage( tw, i, Domain, ImagesDirectory);
            }
            tw.WriteLine("</div>");
            tw.WriteLine("----");
            tw.WriteLine();
            foreach(DirectoryDescriptor d in Directories)
            {
                WriteDirectory( tw, d, ImagesDirectory);
            }
            tw.WriteLine();
            tw.WriteLine("[Gallery]( ./index.html)"); 
            tw.WriteLine("  or ");
            tw.WriteLine("[Home]( ../)");
            tw.WriteLine();
            tw.WriteLine("----");
            tw.WriteLine();
            //if(!DirectoryListing)
            {
                tw.WriteLine("<script>");
                tw.WriteLine();
                tw.WriteLine("{% include single-gallery.js %}");
                tw.WriteLine();
                tw.WriteLine("SetupGallery(\".image-container-" + this.directoryName + " ImgContainer\");");
                tw.WriteLine();
                tw.WriteLine("</script>");
                tw.WriteLine();
            }
            tw.WriteLine("{% include footer.md %}");
            tw.WriteLine();
            tw.Flush();
            tw.Close();
        }

        public void WriteImage(System.IO.TextWriter textWriter, ImageDescriptor id, string Domain, System.IO.DirectoryInfo ImagesDirectory)
        {

            textWriter.WriteLine("<a href=\"" + id.ReSizedFileInfo.FullName.Replace(ImagesDirectory.FullName,"").Replace(@"\","/") + "\" data-fancybox=\"" + id.ThumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"").Replace(@"\","/") + "\" data-caption=\"" + id.Name + "\">");
	        textWriter.WriteLine("    <img class=\"image-thumb\" src=\"" + Domain  + "/" + id.ThumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"").Replace(@"\","/") + "\" alt=\"" + id.Name + "\" />");
            textWriter.WriteLine("</a>");
            //textWriter.WriteLine("[![" + id.Name  + "](" /* + Domain  + "/" */ + id.ThumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"").Replace(@"\","/") + ")](" + id.ReSizedFileInfo.FullName.Replace(ImagesDirectory.FullName,"").Replace(@"\","/") + ")" );
        }
        public void WriteDirectory(System.IO.TextWriter textWriter, DirectoryDescriptor dd, System.IO.DirectoryInfo ImagesDirectory)
        {
            textWriter.WriteLine("### [" + dd.directoryName + "]( ./"+ dd.indexfilename.Name.Replace(".md",".html") + ")"); 

            if(dd.ImageThumbnail != null)
            {
                var inlinelater =  dd.ImageThumbnail.Replace(ImagesDirectory.Name,"").Replace(@"\","/");

                textWriter.WriteLine("[ ![" +  dd.directoryName +  "-Thumbnail](" + inlinelater + ")]( ./"+ dd.indexfilename.Name.Replace(".md",".html") + ")"); 
            }
            textWriter.WriteLine();
        }
    }

    [Serializable()]
    public class ImageDescriptor
    {
        # region Static
        public static string thumbnail = "thumbnail";
        public static string resized = "resized";
        #endregion
        
        
        private int width;
        public int ImageWidth { get => width; set => width = value; }
        private int height;
        public int ImageHeight { get => height; set => height = value; }
        
        private string name;
        public string Name { get => name; set => name = value; }
        private string folder;
        public string Folder { get => folder; set => folder = value; }
        
        [JsonIgnore]
        public FileInfo ImageFile { get => file; }
        [JsonPropertyName("OriginalFilePath")]
        public string JSON_ImageFIle { get => file.FullName.Replace(GitHubRepoRoot.FullName,"");}
        protected FileInfo file;  
        

        protected FileInfo reSizedFileInfo; 
        [JsonIgnore]
        public string ReSizedFileName { get => reSizedFileName; set => reSizedFileName = value; }
        private string reSizedFileName;
        [JsonIgnore]
        public FileInfo ReSizedFileInfo { get => reSizedFileInfo; set => reSizedFileInfo = value; }
        [JsonPropertyName("ResizedFilePath")]
        public string JSON_ResizedImage { get => reSizedFileInfo.FullName.Replace(GitHubRepoRoot.FullName,"");}


        private string thumbnail_name;
        protected System.IO.FileInfo thumbNailFile;
        [JsonIgnore]
        public string ThumbnailName { get => thumbnail_name; }
        [JsonIgnore]
        public FileInfo ThumbNailFile { get => thumbNailFile; set => thumbNailFile = value; }
        [JsonPropertyName("ThumbnailFilePath")]
        public string JSON_ThumbnailFile { get => thumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"");}

        protected DirectoryInfo directory;
        private DirectoryInfo GitHubRepoRoot;

        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;

            thumbnail_name = ImageDescriptor.thumbnail + "-" + directory.Name + "-" + file.Name;
            ReSizedFileName = folder + "\\" +  ImageDescriptor.resized  + "-" + name;
        }

        public void FillBasicInfo()
        {
            var inStream = file.OpenRead();
            using (Image image = Image.Load(inStream))
            {
                height = image.Height;
                width = image.Width;
            }
            inStream.Close();
        }

        public void FixUpPaths(System.IO.DirectoryInfo di)
        {
            this.GitHubRepoRoot = di;

            thumbnail_name = "/" + thumbnail_name.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
            ReSizedFileName = "/" + ReSizedFileName.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
            folder = "/" + folder.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
        }
    }
}
