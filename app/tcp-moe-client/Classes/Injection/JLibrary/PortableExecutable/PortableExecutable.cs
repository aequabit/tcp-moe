namespace JLibrary.PortableExecutable
{
    using JLibrary.Tools;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Serializable]
    public class PortableExecutable : MemoryIterator
    {
        public PortableExecutable(string path) : this(File.ReadAllBytes(path))
        {
            this.FileLocation = path;
        }

        public PortableExecutable(byte[] data) : base(data)
        {
            string str = string.Empty;
            IMAGE_NT_HEADER32 result = new IMAGE_NT_HEADER32();
            IMAGE_DOS_HEADER image_dos_header = new IMAGE_DOS_HEADER();
            if (base.Read<IMAGE_DOS_HEADER>(out image_dos_header) && (image_dos_header.e_magic == 0x5a4d))
            {
                if (base.Read<IMAGE_NT_HEADER32>((long) image_dos_header.e_lfanew, SeekOrigin.Begin, out result) && (result.Signature == 0x4550L))
                {
                    if (result.OptionalHeader.Magic == 0x10b)
                    {
                        if (result.OptionalHeader.DataDirectory[14].Size > 0)
                        {
                            str = "Image contains a CLR runtime header. Currently only native binaries are supported; no .NET dependent libraries.";
                        }
                    }
                    else
                    {
                        str = "File is of the PE32+ format. Currently support only extends to PE32 images. Either recompile the binary as x86, or choose a different target.";
                    }
                }
                else
                {
                    str = "Invalid NT header found in image.";
                }
            }
            else
            {
                str = "Invalid DOS Header found in image";
            }
            if (string.IsNullOrEmpty(str))
            {
                this.NTHeader = result;
                this.DOSHeader = image_dos_header;
            }
            else
            {
                base.Dispose();
                throw new ArgumentException(str);
            }
        }

        public IEnumerable<IMAGE_IMPORT_DESCRIPTOR> EnumImports()
        {
            IMAGE_DATA_DIRECTORY iteratorVariable0 = this.NTHeader.OptionalHeader.DataDirectory[1];
            if (iteratorVariable0.Size > 0)
            {
                IMAGE_IMPORT_DESCRIPTOR iteratorVariable3;
                uint ptrFromRVA = this.GetPtrFromRVA(iteratorVariable0.VirtualAddress);
                uint iteratorVariable2 = typeof(IMAGE_IMPORT_DESCRIPTOR).SizeOf();
                while ((this.Read<IMAGE_IMPORT_DESCRIPTOR>((long) ptrFromRVA, SeekOrigin.Begin, out iteratorVariable3) && (iteratorVariable3.OriginalFirstThunk > 0)) && (iteratorVariable3.Name > 0))
                {
                    yield return iteratorVariable3;
                    ptrFromRVA += iteratorVariable2;
                }
            }
        }

        public IEnumerable<IMAGE_SECTION_HEADER> EnumSectionHeaders()
        {
            uint numberOfSections = this.NTHeader.FileHeader.NumberOfSections;
            long iteratorVariable2 = ((this.NTHeader.FileHeader.SizeOfOptionalHeader + typeof(IMAGE_FILE_HEADER).SizeOf()) + 4) + this.DOSHeader.e_lfanew;
            uint iteratorVariable3 = typeof(IMAGE_SECTION_HEADER).SizeOf();
            for (uint i = 0; i < numberOfSections; i++)
            {
                IMAGE_SECTION_HEADER iteratorVariable0;
                if (this.Read<IMAGE_SECTION_HEADER>(iteratorVariable2 + (i * iteratorVariable3), SeekOrigin.Begin, out iteratorVariable0))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        private IMAGE_SECTION_HEADER GetEnclosingSectionHeader(uint rva)
        {
            foreach (IMAGE_SECTION_HEADER image_section_header in this.EnumSectionHeaders())
            {
                if ((rva >= image_section_header.VirtualAddress) && (rva < (image_section_header.VirtualAddress + ((image_section_header.VirtualSize > 0) ? image_section_header.VirtualSize : image_section_header.SizeOfRawData))))
                {
                    return image_section_header;
                }
            }
            throw new EntryPointNotFoundException("RVA does not exist within any of the current sections.");
        }

        public uint GetPtrFromRVA(uint rva)
        {
            IMAGE_SECTION_HEADER enclosingSectionHeader = this.GetEnclosingSectionHeader(rva);
            return (rva - (enclosingSectionHeader.VirtualAddress - enclosingSectionHeader.PointerToRawData));
        }

        public byte[] ToArray()
        {
            return base.GetUnderlyingData();
        }

        public IMAGE_DOS_HEADER DOSHeader { get; private set; }

        public string FileLocation { get; private set; }

        public IMAGE_NT_HEADER32 NTHeader { get; private set; }


    }
}

