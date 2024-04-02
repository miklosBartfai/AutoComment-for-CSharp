using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AutoCommentExtension
{
    internal partial class OptionsProvider
    {
        // Register the options with this attribute on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "AutoCommentExtension", "General", 0, 0, true, SupportsProfiles = true)]
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>, IXmlCommentOption
    {
        [Category("Auto Run")]
        [DisplayName("Run on save")]
        [Description("Run AutoComment on save.")]
        [DefaultValue(false)]
        public bool RunOnSave { get; set; }

        [Category("Auto Run")]
        [DisplayName("Run on save Command")]
        [Description("Select which command to run on save.\r\n - AutoCommentAll : (Re)generates comments for all public members, even if they already have it.\r\n - AutoCommentMissing : Generates comments only for public members that do not already have them.")]
        [DefaultValue(AutoCommentCommand.AutoCommentMissing)]
        [TypeConverter(typeof(EnumConverter))]
        public AutoCommentCommand RunOnSaveCommand { get; set; } = AutoCommentCommand.AutoCommentMissing;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for public")]
        [Description("If true, comments will be created for public members.")]
        [DefaultValue(true)]
        public bool CreateForPublic { get; set; } = true;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for internal")]
        [Description("If true, comments will be created for internal members.")]
        [DefaultValue(true)]
        public bool CreateForInternal { get; set; } = true;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for protected")]
        [Description("If true, comments will be created for protected members.")]
        [DefaultValue(true)]
        public bool CreateForProtected { get; set; } = true;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for protected internal")]
        [Description("If true, comments will be created for protected internal members.")]
        [DefaultValue(true)]
        public bool CreateForProtectedInternal { get; set; } = true;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for private")]
        [Description("If true, comments will be created for private members.")]
        [DefaultValue(false)]
        public bool CreateForPrivate { get; set; } = false;

        [Category("Access Modifiers")]
        [DisplayName("Create comment for private protected")]
        [Description("If true, comments will be created for private protected members.")]
        [DefaultValue(false)]
        public bool CreateForPrivateProtected { get; set; } = false;

        [Category("Templates")]
        [DisplayName("Class/Struct/Interface/Enum")]
        [Description("The template to use for a class, struct, interface, or enum. Possible values:\r\n - {name} : the name of the class/struct/interface/enum\r\n - {type} : class/struct/interface/enum\r\n - {access} : the access modifier\r\n - {nl} : new line")]
        [DefaultValue(@"/// <summary>{nl}/// The {name} {type}.{nl}/// </summary>")]
        public string ClassTemplate { get; set; } = @"/// <summary>{nl}/// The {name} {type}.{nl}/// </summary>";

        [Category("Templates")]
        [DisplayName("Constructors")]
        [Description("The template to use for the constructors. Possible values:\r\n - {name} : the name of the class/struct\r\n - {access} : the access modifier\r\n - {parameters} : see the 'Parameters' template\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Initializes a new instance of the <see cref=""{name}""/> class.{nl}/// </summary>{parameters}")]
        public string ConstructorTemplate { get; set; } = @"/// <summary>{nl}/// Initializes a new instance of the <see cref=""{name}""/> class.{nl}/// </summary>{parameters}";

        [Category("Templates")]
        [DisplayName("Methods")]
        [Description("The template to use for methods. Possible values:\r\n - {name} : the name of the method\r\n - {access} : the access modifier\r\n - {parameters} : see the 'Parameters' template\r\n - {returns} : see the 'Return value' template\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// {name}.{nl}/// </summary>{parameters}{returns}")]
        public string MethodTemplate { get; set; } = @"/// <summary>{nl}/// {name}.{nl}/// </summary>{parameters}{returns}";

        [Category("Templates")]
        [DisplayName("Parameters")]
        [Description("The template to use for the parameters. Parameters will be placed on a new line. Possible values:\r\n - {name} : the name of the parameter\r\n - {type} : the type of the parameter")]
        [DefaultValue(@"/// <param name=""{name}"">The {name}.</param>")]
        public string ParametersTemplate { get; set; } = @"/// <param name=""{name}"">The {name}.</param>";

        [Category("Templates")]
        [DisplayName("Return value")]
        [Description("The template to use for return value. Return value will be placed on a new line. Possible values:\r\n - {baseType} : the type of the returned value\r\n - {genericType} : if the type is generic, it returns the inner type\r\n - {fullType} : baseType + genericType")]
        [DefaultValue(@"/// <returns>A <see cref=""{baseType}""/>.</returns>")]
        public string ReturnsTemplate { get; set; } = @"/// <returns>A <see cref=""{baseType}""/>.</returns>";

        [Category("Templates")]
        [DisplayName("Property with getter")]
        [Description("The template to use for properties with getter only. Possible values:\r\n - {name} : the name of the property\r\n - {type} : the type of the property\r\n - {access} : the access modifier\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Gets the {name}.{nl}/// </summary>")]
        public string GetTemplate { get; set; } = @"/// <summary>{nl}/// Gets the {name}.{nl}/// </summary>";

        [Category("Templates")]
        [DisplayName("Property with getter and setter")]
        [Description("The template to use for properties with getter and setter. Possible values:\r\n - {name} : the name of the property\r\n - {type} : the type of the property\r\n - {access} : the access modifier\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Gets or sets the {name}.{nl}/// </summary>")]
        public string GetSetTemplate { get; set; } = @"/// <summary>{nl}/// Gets or sets the {name}.{nl}/// </summary>";

        [Category("Templates")]
        [DisplayName("Property with getter and initializer")]
        [Description("The template to use for properties with getter and initializer. Possible values:\r\n - {name} : the name of the property\r\n - {type} : the type of the property\r\n - {access} : the access modifier\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Gets or initializes the {name}.{nl}/// </summary>")]
        public string GetInitTemplate { get; set; } = @"/// <summary>{nl}/// Gets or initializes the {name}.{nl}/// </summary>";

        [Category("Templates")]
        [DisplayName("Property with setter")]
        [Description("The template to use for properties with setter only. Possible values:\r\n - {name} : the name of the property\r\n - {type} : the type of the property\r\n - {access} : the access modifier\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Sets the {name}.{nl}/// </summary>")]
        public string SetTemplate { get; set; } = @"/// <summary>{nl}/// Sets the {name}.{nl}/// </summary>";

        [Category("Templates")]
        [DisplayName("Property with initializer")]
        [Description("The template to use for properties with initializer only. Possible values:\r\n - {name} : the name of the property\r\n - {type} : the type of the property\r\n - {access} : the access modifier\r\n - {nl} for new line.")]
        [DefaultValue(@"/// <summary>{nl}/// Initializes the {name}.{nl}/// </summary>")]
        public string InitTemplate { get; set; } = @"/// <summary>{nl}/// Initializes the {name}.{nl}/// </summary>";
    }
}
