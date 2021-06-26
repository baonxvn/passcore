
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class MDaemon
{

    private MDaemonAPI aPIField;

    /// <remarks/>
    public MDaemonAPI API
    {
        get
        {
            return this.aPIField;
        }
        set
        {
            this.aPIField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPI
{

    private MDaemonAPIResponse responseField;

    /// <remarks/>
    public MDaemonAPIResponse Response
    {
        get
        {
            return this.responseField;
        }
        set
        {
            this.responseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponse
{

    private string serviceNameField;

    private string productVersionField;

    private string serviceVersionField;

    private MDaemonAPIResponseResult resultField;

    private decimal versionField;

    /// <remarks/>
    public string ServiceName
    {
        get
        {
            return this.serviceNameField;
        }
        set
        {
            this.serviceNameField = value;
        }
    }

    /// <remarks/>
    public string ProductVersion
    {
        get
        {
            return this.productVersionField;
        }
        set
        {
            this.productVersionField = value;
        }
    }

    /// <remarks/>
    public string ServiceVersion
    {
        get
        {
            return this.serviceVersionField;
        }
        set
        {
            this.serviceVersionField = value;
        }
    }

    /// <remarks/>
    public MDaemonAPIResponseResult Result
    {
        get
        {
            return this.resultField;
        }
        set
        {
            this.resultField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResult
{

    private MDaemonAPIResponseResultHelp helpField;

    private MDaemonAPIResponseResultOperations operationsField;

    private MDaemonAPIResponseResultReadMeFile readMeFileField;

    private MDaemonAPIResponseResultSampleFile[] sampleFilesField;

    /// <remarks/>
    public MDaemonAPIResponseResultHelp Help
    {
        get
        {
            return this.helpField;
        }
        set
        {
            this.helpField = value;
        }
    }

    /// <remarks/>
    public MDaemonAPIResponseResultOperations Operations
    {
        get
        {
            return this.operationsField;
        }
        set
        {
            this.operationsField = value;
        }
    }

    /// <remarks/>
    public MDaemonAPIResponseResultReadMeFile ReadMeFile
    {
        get
        {
            return this.readMeFileField;
        }
        set
        {
            this.readMeFileField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("SampleFile", IsNullable = false)]
    public MDaemonAPIResponseResultSampleFile[] SampleFiles
    {
        get
        {
            return this.sampleFilesField;
        }
        set
        {
            this.sampleFilesField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResultHelp
{

    private string topicField;

    private string copyrightField;

    /// <remarks/>
    public string Topic
    {
        get
        {
            return this.topicField;
        }
        set
        {
            this.topicField = value;
        }
    }

    /// <remarks/>
    public string Copyright
    {
        get
        {
            return this.copyrightField;
        }
        set
        {
            this.copyrightField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResultOperations
{

    private MDaemonAPIResponseResultOperationsXOption[] xOptionField;

    private string nameField;

    private string typeField;

    private bool requirementField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("xOption")]
    public MDaemonAPIResponseResultOperationsXOption[] xOption
    {
        get
        {
            return this.xOptionField;
        }
        set
        {
            this.xOptionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool requirement
    {
        get
        {
            return this.requirementField;
        }
        set
        {
            this.requirementField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResultOperationsXOption
{

    private string nameField;

    private string typeField;

    private string descriptionField;

    private string securityField;

    private string supercededField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string description
    {
        get
        {
            return this.descriptionField;
        }
        set
        {
            this.descriptionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string security
    {
        get
        {
            return this.securityField;
        }
        set
        {
            this.securityField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string superceded
    {
        get
        {
            return this.supercededField;
        }
        set
        {
            this.supercededField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResultReadMeFile
{

    private string nameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class MDaemonAPIResponseResultSampleFile
{

    private string nameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

