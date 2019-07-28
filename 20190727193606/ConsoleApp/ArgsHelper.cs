using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp
{
    public class ArgHelper
    {
        private readonly string[] _args;

        public ArgHelper(string[] args)
        {
            _args = args;

            LoadArgs();
        }

        public static ArgHelper Load(string[] args)
        {
            return new ArgHelper(args);
        }

        /// <summary>
        /// 运行环境名称,默认支持<see cref="Microsoft.Extensions.Hosting.EnvironmentName"/>的选项
        /// </summary>
        [ArgOption]
        [Description("运行环境名称,可选值为:"
                     + nameof(Microsoft.Extensions.Hosting.EnvironmentName.Development) + "/"
                     + nameof(Microsoft.Extensions.Hosting.EnvironmentName.Staging) + "/"
                     + nameof(Microsoft.Extensions.Hosting.EnvironmentName.Production)
                     + ",默认为:" + nameof(Microsoft.Extensions.Hosting.EnvironmentName.Production))]
        public string EnvironmentName { get; set; } = Microsoft.Extensions.Hosting.EnvironmentName.Production;

        /// <summary>
        /// 显示帮助
        /// </summary>
        [ArgOption]
        [Description("帮助")]
        public string Help { get; set; }

        public bool HasHelp => _args.Any(x =>
                 x.Equals("-h", StringComparison.OrdinalIgnoreCase)
                 || x.Equals("--help", StringComparison.OrdinalIgnoreCase));

        private readonly IEnumerable<PropertyInfo> _propertyInfos =
            typeof(ArgHelper).GetProperties().Where(x => x.GetCustomAttribute<ArgOptionAttribute>() != null).OrderBy(x => x.Name).ToList();

        /// <summary>
        /// 加载参数
        /// </summary>
        /// <param name="args"></param>
        private void LoadArgs()
        {
            if (TryShowHelp())
            {
                return;
            }

            var reg = new Regex(@"(?<key>--?\w+)( (?<value>\w*))?");

            foreach (var arg in reg.Matches(string.Join(" ", _args)).AsEnumerable())
            {
                var argKey = arg.Groups["key"].Value;
                var argValue = arg.Groups["value"].Value;

                PropertyInfo targetInfo;
                switch (argKey.Count(x => x == '-'))
                {
                    case 1:
                        targetInfo = _propertyInfos.FirstOrDefault(x =>
                            x.Name.ToLower().First().Equals(argKey.ToLower().Skip(1).First()));
                        break;
                    case 2:
                        targetInfo = _propertyInfos.FirstOrDefault(x =>
                            x.Name.Equals(argKey.Skip(2).ToString(), StringComparison.OrdinalIgnoreCase));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(argKey), argKey, "无法解析的参数");
                }

                if (targetInfo == null)
                {
                    throw new ArgumentException($"无效的参数: {argKey} {argValue}");
                }

                try
                {
                    var valueType = targetInfo.PropertyType;
                    var valueObj = Convert.ChangeType(argValue, valueType);
                    targetInfo.SetValue(this, valueObj);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"将 {argValue} 转换为 {targetInfo.PropertyType.Name} 失败", e);
                }
            }
        }

        private bool TryShowHelp()
        {
            if (HasHelp)
            {
                var maxPropNameLength = _propertyInfos.Max(x => x.Name.Length);
                //var outputStr = new StringBuilder($"{"".PadLeft(2)} {"".PadLeft(maxPropNameLength + 2)} 描述{Environment.NewLine}");
                var outputStr = new StringBuilder();
                foreach (var propertyInfo in _propertyInfos)
                {
                    var info = propertyInfo;
                    var isFirst = propertyInfo == _propertyInfos.First(x => x.Name.First() == info.Name.First());

                    var sort = isFirst ? propertyInfo.Name.ToLower().First().ToString() : string.Empty;

                    var full = propertyInfo.Name.ToLower().First() +
                                  string.Join("", propertyInfo.Name.Skip(1));

                    var desc = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;

                    outputStr.AppendLine($"{(string.IsNullOrEmpty(sort) ? "  " : "-" + sort)} --{full.PadRight(maxPropNameLength)} {desc}");
                }

                Console.WriteLine(outputStr);

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 表示此属性是一个配置项
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    sealed class ArgOptionAttribute : Attribute
    {
    }
}
