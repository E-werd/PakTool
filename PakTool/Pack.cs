using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PakLib
{
    /// <summary>
    /// Secondary struct for the header of a PAK archive.
    /// </summary>
    struct PackHeader
    {
        #region Member Variables

        public byte[] Head;
        public byte[] IndexOffset;
        public byte[] IndexLength;

        #endregion

        #region Constructor

        /// <summary>
        /// Pack header constructor.  Creates object
        /// for pack header information.
        /// </summary>
        public PackHeader(string dummy)
        {
            Head = new byte[4];
            IndexOffset = new byte[4];
            IndexLength = new byte[4];
        }

        #endregion
    }

    /// <summary>
    /// Secondary class for information about individual files
    /// in a PAK archive.
    /// </summary>
    public class PackFile
    {
        #region Member Variables

        public byte[] FileField;
        public string FileName;
        public string FilePath;
        public Int32 FilePosition;
        public Int32 FileLength;
        public List<string> PathNodes;

        #endregion

        #region Constructor

        /// <summary>
        /// Pack file constructor.  Creates objects
        /// that hold file index information.
        /// </summary>
        public PackFile()
        {
            /* Initialization */
            FileField = new byte[56];
            PathNodes = new List<string>();
        }

        #endregion
    }

    /// <summary>
    /// Primary class for handling PAK archives.
    /// </summary>
    public class Pack : IDisposable
    {
        #region Member Variables

        private FileStream packStream;
        private PackHeader header;
        public PackFile[] Files;
        public string PackName;
        public bool IsValid;
        public int NumFiles;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates Pack object.
        /// </summary>
        /// <param name="pakName">Filename of PAK</param>
        public Pack(string pakName)
        {
            /* Initialization */
            PackName = pakName;
            header = new PackHeader(null);
            try { packStream = new FileStream(PackName, FileMode.OpenOrCreate); }
            catch (UnauthorizedAccessException) { IsValid = false; return; }

            /* Initialize */
            init();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initialization of Pack object.
        /// Checks specified file.
        /// </summary>
        private void init()
        {
            /* Initialization */
            byte[] raw = new byte[12];

            /* Check for empty file */
            if (packStream.Length == 0) createPack();

            /* Read in first 12 bytes and assign */
            packStream.Seek(0, SeekOrigin.Begin);
            packStream.Read(raw, 0, 12);
            for (int i = 0; i < 4; i++) { header.Head[i] = raw[i]; }
            for (int i = 4; i < 8; i++) { header.IndexOffset[i - 4] = raw[i]; }
            for (int i = 8; i < 12; i++) { header.IndexLength[i - 8] = raw[i]; }

            /* Continuity check */
            if (header.Head[0] != 80 || header.Head[1] != 65 || header.Head[2] != 67 || header.Head[3] != 75)
            { IsValid = false; return; } // Header wrong, not a PAK file. Must be 'P' 'A' 'C' 'K'.
            if (BitConverter.ToInt32(header.IndexLength, 0) % 64 != 0) { IsValid = false; return; } // Corrupt index.

            /* Finish up */
            IsValid = true;
            NumFiles = BitConverter.ToInt32(header.IndexLength, 0) / 64;
            readPack();

            /* Reset packStream */
            packStream.Flush();
        }

        /// <summary>
        /// Creates an empty PAK file.
        /// </summary>
        private void createPack()
        {
            /* Prepare header */
            header.Head[0] = 80; // 'P' 80
            header.Head[1] = 65; // 'A' 65
            header.Head[2] = 67; // 'C' 67
            header.Head[3] = 75; // 'K' 75
            header.IndexOffset = BitConverter.GetBytes(12);
            header.IndexLength = BitConverter.GetBytes(0);

            /* Write header */
            packStream.Seek(0, SeekOrigin.Begin);
            packStream.Write(header.Head, 0, 4);
            packStream.Write(header.IndexOffset, 0, 4);
            packStream.Write(header.IndexLength, 0, 4);

            /* Reset packStream */
            packStream.Flush();
        }

        /// <summary>
        /// Reads PAK file, creates file
        /// objects and populates fields.
        /// </summary>
        private void readPack()
        {
            /* Initialization */
            int seekPosition = BitConverter.ToInt32(header.IndexOffset, 0); // start position for file indices, will be incremented later
            byte[] indexData = new byte[64]; // file index data (56 filename, 4 length, 4 offset)
            double totalSize = 0;

            /* Creating file objects */
            Files = new PackFile[NumFiles];
            for (int i = 0; i < NumFiles; i++) { Files[i] = new PackFile(); }

            /* Populating file objects */
            for (int i = 0; i < NumFiles; i++)
            {
                /* Read index data from next position */
                packStream.Seek(seekPosition, SeekOrigin.Begin); // Go to beginning of next index
                packStream.Read(indexData, 0, 64); // Get the entire file entry

                /* Assigning index data */
                for (int j = 0; j < 56; j++) { Files[i].FileField[j] = indexData[j]; }
                Files[i].FilePosition = BitConverter.ToInt32(indexData, 56);
                Files[i].FileLength = BitConverter.ToInt32(indexData, 60);
                totalSize += Files[i].FileLength;

                /* Decoding the FileField to FileName */
                StringBuilder sbFileName = new StringBuilder(56);
                StringBuilder sbFilePath = new StringBuilder(56);
                for (int k = 0; k < 56; k++)
                {
                    try
                    {
                        if ((char)Files[i].FileField[k] == '\0') { break; } // We don't support spaces in names, must be end of filename.
                        else if ((char)Files[i].FileField[k] == '/') // Found a directory
                        {
                            Files[i].PathNodes.Add(sbFileName.ToString());
                            sbFilePath.Append(sbFileName + "/"); // Add new directory to filepath
                            sbFileName.Clear(); // Clear filename, contents were a directory
                        }
                        else sbFileName.Append((char)Files[i].FileField[k]); // Add char-casted-byte to filename string
                    }
                    catch (IndexOutOfRangeException) { break; } // Went too far, assume done.
                }
                Files[i].FileName = sbFileName.ToString();
                Files[i].FilePath = sbFilePath.ToString();

                seekPosition += 64; // Increment to next file index
            }

            /* Reset packStream */
            packStream.Flush();
        }

        /// <summary>
        /// Returns the filename from a path
        /// </summary>
        /// <param name="newfilename">File path to process</param>
        /// <returns>string containing filename</returns>
        private string getFileName(string newfilename)
        {
            StringBuilder sbNewFileName = new StringBuilder();

            for (int i = 0; i < 56; i++)
            {
                try
                {
                    if (newfilename[i] == '/') sbNewFileName.Clear(); // Found directory, clear filename
                    else sbNewFileName.Append(newfilename[i]); // Add char to filename string
                }
                catch (IndexOutOfRangeException) { break; }
            }

            return sbNewFileName.ToString();
        }

        /// <summary>
        /// Once a PackFile's FileName property is filled with a full path,
        /// populates properties FileName, FilePath, FileField.
        /// </summary>
        /// <param name="pfile">a PackFile instance</param>
        private void translateFileName(PackFile pfile)
        {
            /* Decoding the FileField to FileName */
            StringBuilder sbFileName = new StringBuilder(56);
            StringBuilder sbFilePath = new StringBuilder(56);
            for (int i = 0; i < 56; i++)
            {
                try
                {
                    if ((char)pfile.FileName[i] == '\0') { break; } // We don't support spaces in names, must be end of filename.
                    else if ((char)pfile.FileName[i] == '/') // Found a directory
                    {
                        pfile.PathNodes.Add(sbFileName.ToString());
                        sbFilePath.Append(sbFileName + "/"); // Add new directory to filepath
                        sbFileName.Clear(); // Clear filename, contents were a directory
                    }
                    else sbFileName.Append((char)pfile.FileName[i]); // Add char-casted-byte to filename string
                }
                catch (IndexOutOfRangeException) { break; } // Went too far, assume done.
            }
            pfile.FileName = sbFileName.ToString();
            pfile.FilePath = sbFilePath.ToString();

            string fullpath = pfile.FilePath + pfile.FileName;
            for (int i = 0; i < fullpath.Length; i++) pfile.FileField[i] = (byte)fullpath[i];
        }

        /// <summary>
        /// Extends a given array by a given number.
        /// </summary>
        /// <param name="array">Array to extend</param>
        /// <param name="extend">Number to extend by</param>
        /// <returns>Extended array</returns>
        private PackFile[] extendPackArray(PackFile[] array, int extend)
        {
            int newarraysize = array.Length + extend;
            PackFile[] newarray = new PackFile[newarraysize];

            for (int i = 0; i < newarraysize; i++)
            {
                if (newarraysize == 1)
                {
                    newarray[i] = new PackFile();
                    break; // Will cause error otherwise, empty base array
                }
                else if (i > array.Length - 1) newarray[i] = new PackFile();
                else
                {
                    newarray[i] = new PackFile();
                    newarray[i].FileField = array[i].FileField;
                    newarray[i].FileName = array[i].FileName;
                    newarray[i].FilePath = array[i].FilePath;
                    newarray[i].FilePosition = array[i].FilePosition;
                    newarray[i].FileLength = array[i].FileLength;
                }
            }

            return newarray;
        }

        /// <summary>
        /// Walk a directory tree and create a List(PackFile)
        /// </summary>
        /// <param name="target">directory to walk</param>
        /// <param name="currentDir">Uri of current directory</param>
        private void walkDirectoryTree(DirectoryInfo target, Uri currentDir, List<PackFile> newFiles)
        {
            FileInfo[] _files = null;
            DirectoryInfo[] subDirs = null;
            Uri tempUri;

            // First, process all the files directly under this folder
            try { _files = target.GetFiles("*.*"); }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }

            if (_files != null)
            {
                for (int i = 0; i < _files.Length; i++)
                {
                    newFiles.Add(new PackFile());
                    int indexNum = newFiles.Count - 1;
                    tempUri = new Uri(_files[i].FullName);
                    newFiles[indexNum].FileName = currentDir.MakeRelativeUri(tempUri).ToString();
                    translateFileName(newFiles[indexNum]);
                }

                // Now find all the subdirectories under this directory.
                subDirs = target.GetDirectories();
                foreach (DirectoryInfo dirInfo in subDirs) { walkDirectoryTree(dirInfo, currentDir, newFiles); }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Extracts all files from the PAK
        /// archive.
        /// </summary>
        /// <param name="destination">Folder to extract to</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if corrupt index</returns>
        public int ExtractAll(string destination)
        {
            /* Preliminary continuity check */
            if (IsValid != true) return -1;
            else if (NumFiles == 0) return -2;

            /* Create directories and files */
            byte[] buffer;
            for (int i = 0; i < NumFiles; i++)
            {
                if (!Directory.Exists(destination + "/" + Files[i].FilePath))
                    Directory.CreateDirectory(destination + "/" + Files[i].FilePath);

                packStream.Seek(Files[i].FilePosition, SeekOrigin.Begin);
                buffer = new byte[Files[i].FileLength];
                packStream.Read(buffer, 0, Files[i].FileLength);
                File.WriteAllBytes(destination + "/" + Files[i].FilePath + Files[i].FileName, buffer);
            }

            /* Reset packStream */
            packStream.Flush();

            return 1;
        }

        /// <summary>
        /// Extracts specific file from PAK archive, extracts first match.
        /// </summary>
        /// <param name="filename">File to extract</param>
        /// <param name="destination">Folder to extract to</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if corrupt index, -3 if file not found</returns>
        public int ExtractFile(string filename, string destination) 
        {
            int output;
            output = ExtractFile(filename, destination, true);
            return output;
        }

        /// <summary>
        /// Extracts specific file from PAK archive, extracts first match.
        /// </summary>
        /// <param name="filename">File to extract</param>
        /// <param name="destination">Folder to extract to</param>
        /// <param name="keepDirStructure">Maintain directory structure in destination</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if corrupt index, -3 if file not found</returns>
        public int ExtractFile(string filename, string destination, bool keepDirStructure)
        {
            /* Preliminary continuity check */
            if (!IsValid) return -1;
            else if (NumFiles == 0) return -2;

            /* Check for null or empty destination string */
            if (string.IsNullOrEmpty(destination)) destination = Directory.GetCurrentDirectory().ToString();

            /* Create directories and files */
            bool success = false;
            byte[] buffer;

            for (int i = 0; i < NumFiles; i++)
            {
                if (Files[i].FileName == filename || Files[i].FilePath + Files[i].FileName == filename)
                {
                    packStream.Seek(Files[i].FilePosition, SeekOrigin.Begin);
                    buffer = new byte[Files[i].FileLength];
                    packStream.Read(buffer, 0, Files[i].FileLength);

                    if (keepDirStructure)
                    {
                        if (!Directory.Exists(destination + "/" + Files[i].FilePath))
                            Directory.CreateDirectory(destination + "/" + Files[i].FilePath);
                        File.WriteAllBytes(destination + "/" + Files[i].FilePath + Files[i].FileName, buffer);
                    }
                    else
                    {
                        if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
                        File.WriteAllBytes(destination + "/" + Files[i].FileName, buffer);
                    }

                    success = true;
                    break;
                }
                else continue;
            }

            /* Reset packStream */
            packStream.Flush();

            if (!success) return -3;
            return 1;
        }

        /// <summary>
        /// Inserts specified file into PAK file.
        /// </summary>
        /// <param name="filename">File to insert</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if fullpath (destination + filename) is too long, 
        /// -3 if file not found, -4 file already exists</returns>
        public int InsertFile(string filename)
        {
            int output;
            output = InsertFile(filename, null);
            return output;
        }

        /// <summary>
        /// Inserts specified file into PAK file.
        /// </summary>
        /// <param name="filename">File to insert</param>
        /// <param name="destination">Folder to insert into</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if fullpath (destination + filename) is too long, 
        /// -3 if file not found, -4 if file already exists, -5 if file cannot be opened</returns>
        public int InsertFile(string filename, string destination)
        {
            /* Preliminary continuity check */
            if (!IsValid) return -1;

            /* Local var/object creation */
            string actualfilename = getFileName(filename);
            if (!string.IsNullOrEmpty(destination) && destination[destination.Length - 1] != '/') destination += "/";
            string fullpath = destination + actualfilename;

            if (fullpath.Length > 56) return -2; // Too big! Q1/Q2 Limitation

            /* Make sure file doesn't already exist */
            for (int i = 0; i < NumFiles; i++)
                if (Files[i].FilePath + Files[i].FileName == fullpath) return -4; // Already exists!

            /* Open file to be added */
            FileStream newFile;
            try { newFile = new FileStream(filename, FileMode.Open); }
            catch (FileNotFoundException) { return -3; }
            catch (IOException) { return -5; }

            /* Populating file object information */
            Files = extendPackArray(Files, 1);
            int indexNum = Files.Length - 1;
            for (int i = 0; i < fullpath.Length; i++) Files[indexNum].FileField[i] = (byte)fullpath[i]; // Write in name
            Files[indexNum].FileName = actualfilename;
            Files[indexNum].FilePath = destination;
            Files[indexNum].FileLength = (Int32)newFile.Length;
            Files[indexNum].FilePosition = BitConverter.ToInt32(header.IndexOffset, 0);

            header.IndexOffset = BitConverter.GetBytes(BitConverter.ToInt32(header.IndexOffset, 0) + (Int32)newFile.Length);
            header.IndexLength = BitConverter.GetBytes(BitConverter.ToInt32(header.IndexLength, 0) + 64);

            /* Read file into buffer */
            byte[] buffer = new byte[Files[indexNum].FileLength];
            newFile.Read(buffer, 0, Files[indexNum].FileLength);
            newFile.Dispose();

            /* Write buffer to PAK file */
            packStream.Seek(Files[indexNum].FilePosition, SeekOrigin.Begin);
            packStream.Write(buffer, 0, Files[indexNum].FileLength);

            /* Update header information */
            packStream.Seek(4, SeekOrigin.Begin);
            packStream.Write(header.IndexOffset, 0, 4);
            packStream.Write(header.IndexLength, 0, 4);

            /* Rewrite Index */
            int indexOffset = BitConverter.ToInt32(header.IndexOffset, 0);
            for (int i = 0; i < Files.Length; i++)
            {
                packStream.Seek(indexOffset, SeekOrigin.Begin);
                packStream.Write(Files[i].FileField, 0, 56);
                packStream.Write(BitConverter.GetBytes(Files[i].FilePosition), 0, 4);
                packStream.Write(BitConverter.GetBytes(Files[i].FileLength), 0, 4);
                indexOffset += 64;
            }

            NumFiles += 1;

            /* Reset packStream */
            packStream.Flush();

            return 1;
        }

        /// <summary>
        /// Search through a folder and add all files, retaining folder structure.
        /// </summary>
        /// <param name="sourceDir">Directory to search for files</param>
        /// <returns>1 if good, -1 if not a PAK file, -2 if sourceDir null</returns>
        public int InsertFolder(string sourceDir)
        {
            /* Preliminary continuity check */
            if (!IsValid) return -1;
            if (string.IsNullOrEmpty(sourceDir)) return -2;

            /* Save current directory and set to source */
            string curDir = Directory.GetCurrentDirectory();
            Environment.CurrentDirectory = sourceDir;

            /* Local var/object creation */
            DirectoryInfo di = new DirectoryInfo("./");
            Uri currentDir = new Uri(di.FullName);
            List<PackFile> newFiles = new List<PackFile>();

            /* Walk the specified directory tree */
            walkDirectoryTree(di, currentDir, newFiles);

            /* Insert all the new files */
            for (int i = 0; i < newFiles.Count; i++)
                InsertFile(newFiles[i].FilePath + newFiles[i].FileName, newFiles[i].FilePath);

            /* Change back to original directory */
            Environment.CurrentDirectory = curDir;

            return 1;
        }

        /// <summary>
        /// Buffers a specified file from a PAK.
        /// </summary>
        /// <param name="filename">File to buffer</param>
        /// <returns>byte[] if successful, null if not</returns>
        public byte[] GetFileBytes(string filename)
        {
            /* Preliminary continuity check */
            if (!IsValid) return null; // Not a valid PAK
            else if (NumFiles == 0) return null; // No files to work with
            else if (string.IsNullOrEmpty(filename)) return null; // No file specified

            /* Initialization */
            byte[] filebytes = new byte[1];

            /* Find file, get bytes */
            bool success = false;

            for (int i = 0; i < NumFiles; i++)
            {
                if (Files[i].FileName == filename || Files[i].FilePath + Files[i].FileName == filename)
                {
                    filebytes = new byte[Files[i].FileLength];
                    packStream.Seek(Files[i].FilePosition, SeekOrigin.Begin);
                    packStream.Read(filebytes, 0, Files[i].FileLength);
                    success = true;
                    break;
                }
                else continue;
            }

            /* Reset packStream*/
            packStream.Flush();

            if (!success) return null;

            return filebytes;
        }

        #endregion

        #region IDisposable members

        /* For disposal */
        private bool disposed = false;

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        /// <param name="disposing">true if manually disposing, false by finalizer</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed) // if not already disposed
                if (disposing)
                {
                    try { packStream.Dispose(); } 
                    catch (Exception) {  }
                }

            disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Pack() { Dispose(false); }

        #endregion
    }
}