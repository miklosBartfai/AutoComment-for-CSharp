using System.Text;
using System.Text.RegularExpressions;

namespace AutoCommentExtension
{
    internal static class XmlComment
    {
        const string _xmlCommentRegex = @"\s*///";
        const string _attributeRegex = @"\s+\[";

        const string indentRgx = @"(?<indent>\s*)";
        const string accessRgx = @"(?<access>public|internal|protected|protected internal|private|private protected)";
        const string startRgx = indentRgx + accessRgx;
        const string typeRgx = @"(?<type>class|interface|struct|enum)";
        const string nameRgx = @"(?<name>\w+)";
        const string paramRgx = @"(?<param>[^)]*)";
        const string returnRgx = @"(?<baseType>\w+(\?)?)(?<genericType><\w+(\?)?(<\w+>(\?)?)?>)?";

        const string _classRegex = $@"{startRgx}\s+(static\s*)?{typeRgx}\s+{nameRgx}";
        const string _constructorRegex = $@"{startRgx}\s+{nameRgx}\s*\({paramRgx}\)";
        const string _partialConstructorRegex = $@"{startRgx}\s+{nameRgx}\s*\({paramRgx}";
        const string _methodRegex = $@"{startRgx}\s+(static\s*)?(async\s*)?{returnRgx}\s+{nameRgx}(<\w+(,\s*\w+)?>)?\s*\({paramRgx}\)";
        const string _partialMethodRegex = $@"{startRgx}\s+(static\s*)?(async\s*)?{returnRgx}\s+{nameRgx}(<\w+(,\s*\w+)?>)?\s*\({paramRgx}";
        const string _propertyRegex = $@"{startRgx}\s+{returnRgx}\s+{nameRgx}\s*{{\s*(?<getter>get;)?\s*(?<setter>set;)?(?<initer>init;)?\s*}}";
        const string _parameterRegex = @"\s*(?<param>[^)]*)(?<end>\))?";
        const string _lineEnding = "\r\n";

        internal static bool IsXmlComment(string text)
        {
            var match = Regex.Match(text, _xmlCommentRegex);
            return match.Success;
        }

        internal static bool IsAttribute(string text)
        {
            var match = Regex.Match(text, _attributeRegex);
            var firstCharIsAMatch = text.Length > 0 && text[0] == '[';

            return match.Success || firstCharIsAMatch;
        }

        internal static (string Text, bool IsPartial, string NewLine) GetComment(
            string text, IXmlCommentOption option)
        {
            var sb = new StringBuilder();

            var classMatch = Regex.Match(text, _classRegex);

            if (classMatch.Success)
            {
                var access = classMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = classMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var type = classMatch.Groups["type"].Value;
                var name = classMatch.Groups["name"].Value;

                sb.Append(indentation).Append(option.ClassTemplate).Append(_lineEnding);
                sb.Replace("{type}", type)
                    .Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var constructorMatch = Regex.Match(text, _constructorRegex);

            if (constructorMatch.Success)
            {
                var access = constructorMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = constructorMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var name = constructorMatch.Groups["name"].Value;
                var parameters = constructorMatch.Groups["param"].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);

                sb.Append(indentation).Append(option.ConstructorTemplate).Append(_lineEnding);
                sb.Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{parameters}", parametersComment ?? string.Empty)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var partialConstructorMatch = Regex.Match(text, _partialConstructorRegex);

            if (partialConstructorMatch.Success)
            {
                var access = partialConstructorMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = partialConstructorMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var name = partialConstructorMatch.Groups["name"].Value;
                var parameters = partialConstructorMatch.Groups["param"].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var parametersPart = parametersComment ?? string.Empty;
                parametersPart += "{additional parameters}";

                sb.Append(indentation).Append(option.ConstructorTemplate).Append(_lineEnding);
                sb.Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{parameters}", parametersPart)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), true, newLine);
            }

            var methodMatch = Regex.Match(text, _methodRegex);

            if (methodMatch.Success)
            {
                var access = methodMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = methodMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var baseType = methodMatch.Groups["baseType"].Value;
                var generic = methodMatch.Groups["genericType"].Value;
                var genericType = generic.TrimStart('<').TrimEnd('>');
                var fullType = baseType + generic;
                var name = methodMatch.Groups["name"].Value;
                var parameters = methodMatch.Groups["param"].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var returnsComment = GetCommentForReturnValue(baseType, newLine, option);

                sb.Append(indentation).Append(option.MethodTemplate).Append(_lineEnding);
                sb.Replace("{parameters}", parametersComment ?? string.Empty)
                    .Replace("{returns}", returnsComment ?? string.Empty)
                    .Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{baseType}", baseType)
                    .Replace("{genericType}", genericType)
                    .Replace("{fullType}", fullType)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var partialMethodMatch = Regex.Match(text, _partialMethodRegex);

            if (partialMethodMatch.Success)
            {
                var access = partialMethodMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = partialMethodMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var baseType = partialMethodMatch.Groups["baseType"].Value;
                var generic = partialMethodMatch.Groups["genericType"].Value;
                var genericType = generic.TrimStart('<').TrimEnd('>');
                var fullType = baseType + generic;
                var name = partialMethodMatch.Groups["name"].Value;
                var parameters = partialMethodMatch.Groups["param"].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var returnsComment = GetCommentForReturnValue(baseType, newLine, option);
                var parametersPart = parametersComment ?? string.Empty;
                parametersPart += "{additional parameters}";

                sb.Append(indentation).Append(option.MethodTemplate).Append(_lineEnding);
                sb.Replace("{parameters}", parametersPart)
                    .Replace("{returns}", returnsComment ?? string.Empty)
                    .Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{baseType}", baseType)
                    .Replace("{genericType}", genericType)
                    .Replace("{fullType}", fullType)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), true, newLine);
            }

            var propertyMatch = Regex.Match(text, _propertyRegex);

            if (propertyMatch.Success)
            {
                var access = propertyMatch.Groups["access"].Value;

                if (accessModifierIsNotEnabledFor(access, option))
                {
                    return (null, false, string.Empty);
                }

                var indentation = propertyMatch.Groups["indent"].Value;
                var newLine = _lineEnding + indentation;
                var baseType = propertyMatch.Groups["baseType"].Value;
                var generic = propertyMatch.Groups["genericType"].Value;
                var genericType = generic.TrimStart('<').TrimEnd('>');
                var fullType = baseType + generic;
                var name = propertyMatch.Groups["name"].Value;
                var hasGetter = propertyMatch.Groups["getter"].Success;
                var hasSetter = propertyMatch.Groups["setter"].Success;
                var hasIniter = propertyMatch.Groups["initer"].Success;

                var template = GetPropetyTemplate(hasGetter, hasSetter, hasIniter, option);

                sb.Append(indentation).Append(template).Append(_lineEnding);
                sb.Replace("{name}", name)
                    .Replace("{access}", access)
                    .Replace("{baseType}", baseType)
                    .Replace("{genericType}", genericType)
                    .Replace("{fullType}", fullType)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            return (null, false, string.Empty);
        }

        internal static (string Text, bool IsPartial, string newLine) GetPartialComment(
            string text, IXmlCommentOption option, string newLine)
        {
            var sb = new StringBuilder();

            var parameterMatch = Regex.Match(text, _parameterRegex);

            if (parameterMatch.Success)
            {
                var parameters = parameterMatch.Groups["param"].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                sb.Append(parametersComment);

                var lastLine = parameterMatch.Groups["end"].Success;

                return (sb.ToString(), !lastLine, newLine);
            }

            return (null, false, string.Empty);
        }

        private static string GetCommentForParameters(string text, string newLine, IXmlCommentOption option)
        {
            var parameters = text.Split(',');
            var sb = new StringBuilder();

            foreach (var parameter in parameters)
            {
                var typeName = parameter.Trim().Split(' ');

                if (typeName.Length < 2)
                {
                    continue;
                }

                var (type, name) = typeName.Length switch
                {
                    2 => (typeName[0], typeName[1]),
                    3 => (typeName[1], typeName[2]),
                    _ => ("-error-", "-error-")
                };

                var parameterLine = newLine + option.ParametersTemplate;
                parameterLine = parameterLine.Replace("{type}", type).Replace("{name}", name).Replace("{nl}", newLine);

                sb.Append(parameterLine);
            }

            return sb.ToString();
        }

        private static string GetCommentForReturnValue(string type, string newLine, IXmlCommentOption option)
        {
            if (type == "void")
            {
                return null;
            }

            var template = option.ReturnsTemplate;
            return newLine + template;
        }

        private static bool accessModifierIsNotEnabledFor(string access, IXmlCommentOption option)
        {
            return access switch
            {
                "public" => !option.CreateForPublic,
                "internal" => !option.CreateForInternal,
                "protected" => !option.CreateForProtected,
                "protected internal" => !option.CreateForProtectedInternal,
                "private" => !option.CreateForPrivate,
                "private protected" => !option.CreateForPrivateProtected,
                _ => false
            };
        }

        private static string GetPropetyTemplate(bool hasGetter, bool hasSetter, bool hasIniter, IXmlCommentOption option)
        {
            var tuple = (hasGetter, hasSetter, hasIniter);

            return tuple switch
            {
                (true, false, false) => option.GetTemplate,
                (true, true, false) => option.GetSetTemplate,
                (true, false, true) => option.GetInitTemplate,
                (false, true, false) => option.SetTemplate,
                (false, false, true) => option.InitTemplate,
                _ => null,
            };
        }
    }
}
