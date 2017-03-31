namespace JLibrary.PortableExecutable
{
    using System;

    public enum DATA_DIRECTORIES
    {
        ExportTable,
        ImportTable,
        ResourceTable,
        ExceptionTable,
        CertificateTable,
        BaseRelocTable,
        Debug,
        Architecture,
        GlobalPtr,
        TLSTable,
        LoadConfigTable,
        BoundImport,
        IAT,
        DelayImportDescriptor,
        CLRRuntimeHeader,
        Reserved
    }
}

