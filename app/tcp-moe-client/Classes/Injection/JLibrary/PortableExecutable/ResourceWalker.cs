namespace JLibrary.PortableExecutable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class ResourceWalker
    {
        public ResourceWalker(JLibrary.PortableExecutable.PortableExecutable image)
        {
            IMAGE_DATA_DIRECTORY image_data_directory = image.NTHeader.OptionalHeader.DataDirectory[2];
            uint root = 0;
            if ((image_data_directory.VirtualAddress > 0) && (image_data_directory.Size > 0))
            {
                IMAGE_RESOURCE_DIRECTORY image_resource_directory;
                if (!image.Read<IMAGE_RESOURCE_DIRECTORY>((long) (root = image.GetPtrFromRVA(image_data_directory.VirtualAddress)), SeekOrigin.Begin, out image_resource_directory))
                {
                    throw image.GetLastError();
                }
                IMAGE_RESOURCE_DIRECTORY_ENTRY entry = new IMAGE_RESOURCE_DIRECTORY_ENTRY {
                    SubdirectoryRva = 0x80000000
                };
                this.Root = new ResourceDirectory(image, entry, false, root);
            }
        }

        public ResourceDirectory Root { get; private set; }

        public class ResourceDirectory : ResourceWalker.ResourceObject
        {
            private IMAGE_RESOURCE_DIRECTORY _base;
            private ResourceWalker.ResourceDirectory[] _dirs;
            private ResourceWalker.ResourceFile[] _files;
            private const uint SZ_DIRECTORY = 0x10;
            private const uint SZ_ENTRY = 8;

            public ResourceDirectory(JLibrary.PortableExecutable.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root) : base(owner, entry, named, root)
            {
                if (!owner.Read<IMAGE_RESOURCE_DIRECTORY>((long) (root + (entry.SubdirectoryRva ^ 0x80000000)), SeekOrigin.Begin, out this._base))
                {
                    throw owner.GetLastError();
                }
            }

            private void Initialize()
            {
                List<ResourceWalker.ResourceDirectory> list = new List<ResourceWalker.ResourceDirectory>();
                List<ResourceWalker.ResourceFile> list2 = new List<ResourceWalker.ResourceFile>();
                int numberOfNamedEntries = this._base.NumberOfNamedEntries;
                for (int i = 0; i < (numberOfNamedEntries + this._base.NumberOfIdEntries); i++)
                {
                    IMAGE_RESOURCE_DIRECTORY_ENTRY image_resource_directory_entry;
                    if (base._owner.Read<IMAGE_RESOURCE_DIRECTORY_ENTRY>(((base._root + 0x10) + (this._entry.SubdirectoryRva ^ 0x80000000)) + (i * 8L), SeekOrigin.Begin, out image_resource_directory_entry))
                    {
                        if ((image_resource_directory_entry.SubdirectoryRva & 0x80000000) != 0)
                        {
                            list.Add(new ResourceWalker.ResourceDirectory(base._owner, image_resource_directory_entry, i < numberOfNamedEntries, base._root));
                        }
                        else
                        {
                            list2.Add(new ResourceWalker.ResourceFile(base._owner, image_resource_directory_entry, i < numberOfNamedEntries, base._root));
                        }
                    }
                }
                this._files = list2.ToArray();
                this._dirs = list.ToArray();
            }

            public ResourceWalker.ResourceDirectory[] Directories
            {
                get
                {
                    if (this._dirs == null)
                    {
                        this.Initialize();
                    }
                    return this._dirs;
                }
            }

            public ResourceWalker.ResourceFile[] Files
            {
                get
                {
                    if (this._files == null)
                    {
                        this.Initialize();
                    }
                    return this._files;
                }
            }
        }

        public class ResourceFile : ResourceWalker.ResourceObject
        {
            private IMAGE_RESOURCE_DATA_ENTRY _base;

            public ResourceFile(JLibrary.PortableExecutable.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root) : base(owner, entry, named, root)
            {
                if (!owner.Read<IMAGE_RESOURCE_DATA_ENTRY>((long) (base._root + entry.DataEntryRva), SeekOrigin.Begin, out this._base))
                {
                    throw owner.GetLastError();
                }
            }

            public byte[] GetData()
            {
                byte[] buffer = new byte[this._base.Size];
                if (!base._owner.Read((long) base._owner.GetPtrFromRVA(this._base.OffsetToData), SeekOrigin.Begin, buffer))
                {
                    throw base._owner.GetLastError();
                }
                return buffer;
            }
        }

        public abstract class ResourceObject
        {
            protected IMAGE_RESOURCE_DIRECTORY_ENTRY _entry;
            private string _name;
            protected JLibrary.PortableExecutable.PortableExecutable _owner;
            protected uint _root;

            public ResourceObject(JLibrary.PortableExecutable.PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root)
            {
                this._owner = owner;
                this._entry = entry;
                this.IsNamedResource = named;
                if (named)
                {
                    ushort result = 0;
                    if (owner.Read<ushort>((long) (root + (entry.NameRva & 0x7fffffff)), SeekOrigin.Begin, out result))
                    {
                        byte[] buffer = new byte[result << 1];
                        if (owner.Read(0L, SeekOrigin.Current, buffer))
                        {
                            this._name = Encoding.Unicode.GetString(buffer);
                        }
                    }
                    if (this._name == null)
                    {
                        throw owner.GetLastError();
                    }
                }
                this._root = root;
            }

            public int Id
            {
                get
                {
                    return (this.IsNamedResource ? -1 : ((int) this._entry.IntegerId));
                }
            }

            public bool IsNamedResource { get; protected set; }

            public string Name
            {
                get
                {
                    return this._name;
                }
            }
        }
    }
}

