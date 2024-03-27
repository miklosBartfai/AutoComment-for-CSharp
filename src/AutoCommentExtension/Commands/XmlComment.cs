using System.Text;
using System.Text.RegularExpressions;

namespace AutoCommentExtension
{
    internal static class XmlComment
    {
        const string _xmlCommentRegex = @"\s*///";
        const string _attributeRegex = @"\s+\[";
        const string _classRegex = @"(\s*)public\s+(class|interface|struct|enum)\s+(\w+)";
        const string _constructorRegex = @"(\s*)public\s+(\w+)\s*\(([^)]*)\)";
        const string _partialConstructorRegex = @"(\s*)public\s+(\w+)\s*\(([^)]*)[^\)]";
        const string _methodRegex = @"(\s*)public\s+(static\s*)?(async\s*)?(\w+(<\w+>)?)\s+(\w+)\s*\(([^)]*)\)";
        const string _partialMethodRegex = @"(\s*)public\s+(static\s*)?(async\s*)?(\w+(<\w+>)?)\s+(\w+)\s*\(([^)]*)[^\)]";
        const string _propertyRegex = @"(\s*)public\s+(\w+)\s+(\w+)\s*{\s*(get;)?\s*(set;)?(init;)?\s*}";
        const string _parameterRegex = @"\s*([^)]*)(\))?";
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
                var indentation = classMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var type = classMatch.Groups[2].Value;
                var name = classMatch.Groups[3].Value;
                var inheritances = classMatch.Groups[5].Value;

                sb.Append(indentation).Append(option.ClassTemplate).Append(_lineEnding);
                sb.Replace("{type}", type).Replace("{name}", name).Replace("{inheritances}", inheritances)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var constructorMatch = Regex.Match(text, _constructorRegex);

            if (constructorMatch.Success)
            {
                var indentation = constructorMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var name = constructorMatch.Groups[2].Value;
                var parameters = constructorMatch.Groups[3].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);

                sb.Append(indentation).Append(option.ConstructorTemplate).Append(_lineEnding);
                sb.Replace("{name}", name).Replace("{parameters}", parametersComment ?? string.Empty)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var partialConstructorMatch = Regex.Match(text, _partialConstructorRegex);

            if (partialConstructorMatch.Success)
            {
                var indentation = partialConstructorMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var name = partialConstructorMatch.Groups[2].Value;
                var parameters = partialConstructorMatch.Groups[3].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var parametersPart = parametersComment ?? string.Empty;
                parametersPart += "{additional parameters}";

                sb.Append(indentation).Append(option.ConstructorTemplate).Append(_lineEnding);
                sb.Replace("{name}", name).Replace("{parameters}", parametersPart)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), true, newLine);
            }

            var methodMatch = Regex.Match(text, _methodRegex);

            if (methodMatch.Success)
            {
                var indentation = methodMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var type = methodMatch.Groups[4].Value;
                var name = methodMatch.Groups[6].Value;
                var parameters = methodMatch.Groups[7].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var returnsComment = GetCommentForReturnValue(type, newLine, option);

                sb.Append(indentation).Append(option.MethodTemplate).Append(_lineEnding);
                sb.Replace("{parameters}", parametersComment ?? string.Empty)
                    .Replace("{returns}", returnsComment ?? string.Empty)
                    .Replace("{type}", type)
                    .Replace("{name}", name)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), false, newLine);
            }

            var partialMethodMatch = Regex.Match(text, _partialMethodRegex);

            if (partialMethodMatch.Success)
            {
                var indentation = partialMethodMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var type = partialMethodMatch.Groups[4].Value;
                var name = partialMethodMatch.Groups[6].Value;
                var parameters = partialMethodMatch.Groups[7].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                var returnsComment = GetCommentForReturnValue(type, newLine, option);
                var parametersPart = parametersComment ?? string.Empty;
                parametersPart += "{additional parameters}";

                sb.Append(indentation).Append(option.MethodTemplate).Append(_lineEnding);
                sb.Replace("{parameters}", parametersPart)
                    .Replace("{returns}", returnsComment ?? string.Empty)
                    .Replace("{type}", type)
                    .Replace("{name}", name)
                    .Replace("{nl}", newLine);

                return (sb.ToString(), true, newLine);
            }

            var propertyMatch = Regex.Match(text, _propertyRegex);

            if (propertyMatch.Success)
            {
                var indentation = propertyMatch.Groups[1].Value;
                var newLine = _lineEnding + indentation;
                var type = propertyMatch.Groups[2].Value;
                var name = propertyMatch.Groups[3].Value;
                var hasGetter = propertyMatch.Groups[4].Success;
                var hasSetter = propertyMatch.Groups[5].Success;
                var hasIniter = propertyMatch.Groups[6].Success;

                var template = GetPropetyTemplate(hasGetter, hasSetter, hasIniter, option);

                sb.Append(indentation).Append(template).Append(_lineEnding);
                sb.Replace("{type}", type).Replace("{name}", name).Replace("{nl}", newLine);

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
                var parameters = parameterMatch.Groups[1].Value;
                var parametersComment = GetCommentForParameters(parameters, newLine, option);
                sb.Append(parametersComment);

                var lastLine = parameterMatch.Groups[2].Success;

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

                if (typeName.Length != 2)
                {
                    continue;
                }

                var type = typeName[0];
                var name = typeName[1];

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
