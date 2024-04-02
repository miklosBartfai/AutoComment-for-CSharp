namespace AutoCommentExtension
{
    internal interface IXmlCommentOption
    {
        bool RunOnSave { get; set; }
        AutoCommentCommand RunOnSaveCommand { get; set; }
        bool CreateForPublic { get; set; }
        bool CreateForInternal { get; set; }
        bool CreateForProtected { get; set; }
        bool CreateForProtectedInternal { get; set; }
        bool CreateForPrivate { get; set; }
        bool CreateForPrivateProtected { get; set; }
        string ClassTemplate { get; set; }
        string ConstructorTemplate { get; set; }
        string MethodTemplate { get; set; }
        string ParametersTemplate { get; set; }
        string ReturnsTemplate { get; set; }
        string GetTemplate { get; set; }
        string GetSetTemplate { get; set; }
        string GetInitTemplate { get; set; }
        string SetTemplate { get; set; }
        string InitTemplate { get; set; }
    }
}
