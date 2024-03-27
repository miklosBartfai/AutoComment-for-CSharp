namespace AutoCommentExtension
{
    internal interface IXmlCommentOption
    {
        bool RunOnSave { get; set; }
        AutoCommentCommand RunOnSaveCommand { get; set; }
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
