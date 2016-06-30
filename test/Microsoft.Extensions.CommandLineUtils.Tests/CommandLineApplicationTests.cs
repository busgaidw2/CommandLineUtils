// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Xunit;

namespace Microsoft.Extensions.Internal
{
    public class CommandLineApplicationTests
    {
        [Fact]
        public void CommandNameCanBeMatched()
        {
            var called = false;

            var app = new CommandLineApplication();
            app.Command("test", c =>
            {
                c.OnExecute(() =>
                {
                    called = true;
                    return 5;
                });
            });

            var result = app.Execute("test");
            Assert.Equal(5, result);
            Assert.True(called);
        }

        [Fact]
        public void RemainingArgsArePassed()
        {
            CommandArgument first = null;
            CommandArgument second = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Argument("first", "First argument");
                second = c.Argument("second", "Second argument");
                c.OnExecute(() => 0);
            });

            app.Execute("test", "one", "two");

            Assert.Equal("one", first.Value);
            Assert.Equal("two", second.Value);
        }

        [Fact]
        public void ExtraArgumentCausesException()
        {
            CommandArgument first = null;
            CommandArgument second = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Argument("first", "First argument");
                second = c.Argument("second", "Second argument");
                c.OnExecute(() => 0);
            });

            var ex = Assert.Throws<CommandParsingException>(() => app.Execute("test", "one", "two", "three"));

            Assert.Contains("three", ex.Message);
        }

        [Fact]
        public void UnknownCommandCausesException()
        {
            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                c.Argument("first", "First argument");
                c.Argument("second", "Second argument");
                c.OnExecute(() => 0);
            });

            var ex = Assert.Throws<CommandParsingException>(() => app.Execute("test2", "one", "two", "three"));

            Assert.Contains("test2", ex.Message);
        }

        [Fact]
        public void MultipleValuesArgumentConsumesAllArgumentValues()
        {
            CommandArgument argument = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                argument = c.Argument("arg", "Argument that allows multiple values", multipleValues: true);
                c.OnExecute(() => 0);
            });

            app.Execute("test", "one", "two", "three", "four", "five");

            Assert.Equal(new[] { "one", "two", "three", "four", "five" }, argument.Values);
        }

        [Fact]
        public void MultipleValuesArgumentConsumesAllRemainingArgumentValues()
        {
            CommandArgument first = null;
            CommandArgument second = null;
            CommandArgument third = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Argument("first", "First argument");
                second = c.Argument("second", "Second argument");
                third = c.Argument("third", "Third argument that allows multiple values", multipleValues: true);
                c.OnExecute(() => 0);
            });

            app.Execute("test", "one", "two", "three", "four", "five");

            Assert.Equal("one", first.Value);
            Assert.Equal("two", second.Value);
            Assert.Equal(new[] { "three", "four", "five" }, third.Values);
        }

        [Fact]
        public void MultipleValuesArgumentMustBeTheLastArgument()
        {
            var app = new CommandLineApplication();
            app.Argument("first", "First argument", multipleValues: true);
            var ex = Assert.Throws<InvalidOperationException>(() => app.Argument("second", "Second argument"));

            Assert.Contains($"The last argument 'first' accepts multiple values. No more argument can be added.",
                ex.Message);
        }

        [Fact]
        public void OptionSwitchMayBeProvided()
        {
            CommandOption first = null;
            CommandOption second = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Option("--first <NAME>", "First argument", CommandOptionType.SingleValue);
                second = c.Option("--second <NAME>", "Second argument", CommandOptionType.SingleValue);
                c.OnExecute(() => 0);
            });

            app.Execute("test", "--first", "one", "--second", "two");

            Assert.Equal("one", first.Values[0]);
            Assert.Equal("two", second.Values[0]);
        }

        [Fact]
        public void OptionValueMustBeProvided()
        {
            CommandOption first = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Option("--first <NAME>", "First argument", CommandOptionType.SingleValue);
                c.OnExecute(() => 0);
            });

            var ex = Assert.Throws<CommandParsingException>(() => app.Execute("test", "--first"));

            Assert.Contains($"Missing value for option '{first.LongName}'", ex.Message);
        }

        [Fact]
        public void ValuesMayBeAttachedToSwitch()
        {
            CommandOption first = null;
            CommandOption second = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Option("--first <NAME>", "First argument", CommandOptionType.SingleValue);
                second = c.Option("--second <NAME>", "Second argument", CommandOptionType.SingleValue);
                c.OnExecute(() => 0);
            });

            app.Execute("test", "--first=one", "--second:two");

            Assert.Equal("one", first.Values[0]);
            Assert.Equal("two", second.Values[0]);
        }

        [Fact]
        public void ShortNamesMayBeDefined()
        {
            CommandOption first = null;
            CommandOption second = null;

            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                first = c.Option("-1 --first <NAME>", "First argument", CommandOptionType.SingleValue);
                second = c.Option("-2 --second <NAME>", "Second argument", CommandOptionType.SingleValue);
                c.OnExecute(() => 0);
            });

            app.Execute("test", "-1=one", "-2", "two");

            Assert.Equal("one", first.Values[0]);
            Assert.Equal("two", second.Values[0]);
        }

        [Fact]
        public void ThrowsExceptionOnUnexpectedCommandOrArgumentByDefault()
        {
            var unexpectedArg = "UnexpectedArg";
            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            });

            var exception = Assert.Throws<CommandParsingException>(() => app.Execute("test", unexpectedArg));
            Assert.Equal($"Unrecognized command or argument '{unexpectedArg}'", exception.Message);
        }

        [Fact]
        public void AllowNoThrowBehaviorOnUnexpectedArgument()
        {
            var unexpectedArg = "UnexpectedArg";
            var app = new CommandLineApplication();

            var testCmd = app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            },
            throwOnUnexpectedArg: false);

            // (does not throw)
            app.Execute("test", unexpectedArg);
            Assert.Equal(1, testCmd.RemainingArguments.Count);
            Assert.Equal(unexpectedArg, testCmd.RemainingArguments[0]);
        }

        [Fact]
        public void ThrowsExceptionOnUnexpectedLongOptionByDefault()
        {
            var unexpectedOption = "--UnexpectedOption";
            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            });

            var exception = Assert.Throws<CommandParsingException>(() => app.Execute("test", unexpectedOption));
            Assert.Equal($"Unrecognized option '{unexpectedOption}'", exception.Message);
        }

        [Fact]
        public void AllowNoThrowBehaviorOnUnexpectedLongOption()
        {
            var unexpectedOption = "--UnexpectedOption";
            var app = new CommandLineApplication();

            var testCmd = app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            },
            throwOnUnexpectedArg: false);

            // (does not throw)
            app.Execute("test", unexpectedOption);
            Assert.Equal(1, testCmd.RemainingArguments.Count);
            Assert.Equal(unexpectedOption, testCmd.RemainingArguments[0]);
        }

        [Fact]
        public void ThrowsExceptionOnUnexpectedShortOptionByDefault()
        {
            var unexpectedOption = "-uexp";
            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            });

            var exception = Assert.Throws<CommandParsingException>(() => app.Execute("test", unexpectedOption));
            Assert.Equal($"Unrecognized option '{unexpectedOption}'", exception.Message);
        }

        [Fact]
        public void AllowNoThrowBehaviorOnUnexpectedShortOption()
        {
            var unexpectedOption = "-uexp";
            var app = new CommandLineApplication();

            var testCmd = app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            },
            throwOnUnexpectedArg: false);

            // (does not throw)
            app.Execute("test", unexpectedOption);
            Assert.Equal(1, testCmd.RemainingArguments.Count);
            Assert.Equal(unexpectedOption, testCmd.RemainingArguments[0]);
        }

        [Fact]
        public void ThrowsExceptionOnUnexpectedSymbolOptionByDefault()
        {
            var unexpectedOption = "-?";
            var app = new CommandLineApplication();

            app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            });

            var exception = Assert.Throws<CommandParsingException>(() => app.Execute("test", unexpectedOption));
            Assert.Equal($"Unrecognized option '{unexpectedOption}'", exception.Message);
        }

        [Fact]
        public void AllowNoThrowBehaviorOnUnexpectedSymbolOption()
        {
            var unexpectedOption = "-?";
            var app = new CommandLineApplication();

            var testCmd = app.Command("test", c =>
            {
                c.OnExecute(() => 0);
            },
            throwOnUnexpectedArg: false);

            // (does not throw)
            app.Execute("test", unexpectedOption);
            Assert.Equal(1, testCmd.RemainingArguments.Count);
            Assert.Equal(unexpectedOption, testCmd.RemainingArguments[0]);
        }

        [Fact]
        public void ThrowsExceptionOnUnexpectedOptionBeforeValidSubcommandByDefault()
        {
            var unexpectedOption = "--unexpected";
            CommandLineApplication subCmd = null;
            var app = new CommandLineApplication();

            app.Command("k", c =>
            {
                subCmd = c.Command("run", _ => { });
                c.OnExecute(() => 0);
            });

            var exception = Assert.Throws<CommandParsingException>(() => app.Execute("k", unexpectedOption, "run"));
            Assert.Equal($"Unrecognized option '{unexpectedOption}'", exception.Message);
        }

        [Fact]
        public void AllowNoThrowBehaviorOnUnexpectedOptionAfterSubcommand()
        {
            var unexpectedOption = "--unexpected";
            CommandLineApplication subCmd = null;
            var app = new CommandLineApplication();

            var testCmd = app.Command("k", c =>
            {
                subCmd = c.Command("run", _ => { }, throwOnUnexpectedArg: false);
                c.OnExecute(() => 0);
            });

            // (does not throw)
            app.Execute("k", "run", unexpectedOption);
            Assert.Equal(0, testCmd.RemainingArguments.Count);
            Assert.Equal(1, subCmd.RemainingArguments.Count);
            Assert.Equal(unexpectedOption, subCmd.RemainingArguments[0]);
        }

        [Fact]
        public void OptionsCanBeInherited()
        {
            var app = new CommandLineApplication();
            var optionA = app.Option("-a|--option-a", "", CommandOptionType.SingleValue, inherited: true);
            string optionAValue = null;

            var optionB = app.Option("-b", "", CommandOptionType.SingleValue, inherited: false);

            var subcmd = app.Command("subcmd", c =>
            {
                c.OnExecute(() =>
                {
                    optionAValue = optionA.Value();
                    return 0;
                });
            });

            Assert.Equal(2, app.GetOptions().Count());
            Assert.Equal(1, subcmd.GetOptions().Count());

            app.Execute("-a", "A1", "subcmd");
            Assert.Equal("A1", optionAValue);

            Assert.Throws<CommandParsingException>(() => app.Execute("subcmd", "-b", "B"));

            Assert.Contains("-a|--option-a", subcmd.GetHelpText());
        }

        [Fact]
        public void NestedOptionConflictThrows()
        {
            var app = new CommandLineApplication();
            app.Option("-a|--always", "Top-level", CommandOptionType.SingleValue, inherited: true);
            app.Command("subcmd", c =>
            {
                c.Option("-a|--ask", "Nested", CommandOptionType.SingleValue);
            });

            Assert.Throws<InvalidOperationException>(() => app.Execute("subcmd", "-a", "b"));
        }

        [Fact]
        public void OptionsWithSameName()
        {
            var app = new CommandLineApplication();
            var top = app.Option("-a|--always", "Top-level", CommandOptionType.SingleValue, inherited: false);
            CommandOption nested = null;
            app.Command("subcmd", c =>
            {
                nested = c.Option("-a|--ask", "Nested", CommandOptionType.SingleValue);
            });

            app.Execute("-a", "top");
            Assert.Equal("top", top.Value());
            Assert.Null(nested.Value());

            top.Values.Clear();

            app.Execute("subcmd", "-a", "nested");
            Assert.Null(top.Value());
            Assert.Equal("nested", nested.Value());
        }


        [Fact]
        public void NestedInheritedOptions()
        {
            string globalOptionValue = null, nest1OptionValue = null, nest2OptionValue = null;

            var app = new CommandLineApplication();
            CommandLineApplication subcmd2 = null;
            var g = app.Option("-g|--global", "Global option", CommandOptionType.SingleValue, inherited: true);
            var subcmd1 = app.Command("lvl1", s1 =>
            {
                var n1 = s1.Option("--nest1", "Nested one level down", CommandOptionType.SingleValue, inherited: true);
                subcmd2 = s1.Command("lvl2", s2 =>
                {
                    var n2 = s2.Option("--nest2", "Nested one level down", CommandOptionType.SingleValue, inherited: true);
                    s2.HelpOption("-h|--help");
                    s2.OnExecute(() =>
                    {
                        globalOptionValue = g.Value();
                        nest1OptionValue = n1.Value();
                        nest2OptionValue = n2.Value();
                        return 0;
                    });
                });
            });

            Assert.False(app.GetOptions().Any(o => o.LongName == "nest2"));
            Assert.False(app.GetOptions().Any(o => o.LongName == "nest1"));
            Assert.Contains(app.GetOptions(), o => o.LongName == "global");

            Assert.False(subcmd1.GetOptions().Any(o => o.LongName == "nest2"));
            Assert.Contains(subcmd1.GetOptions(), o => o.LongName == "nest1");
            Assert.Contains(subcmd1.GetOptions(), o => o.LongName == "global");

            Assert.Contains(subcmd2.GetOptions(), o => o.LongName == "nest2");
            Assert.Contains(subcmd2.GetOptions(), o => o.LongName == "nest1");
            Assert.Contains(subcmd2.GetOptions(), o => o.LongName == "global");

            Assert.Throws<CommandParsingException>(() => app.Execute("--nest2", "N2", "--nest1", "N1", "-g", "G"));
            Assert.Throws<CommandParsingException>(() => app.Execute("lvl1", "--nest2", "N2", "--nest1", "N1", "-g", "G"));

            app.Execute("lvl1", "lvl2", "--nest2", "N2", "-g", "G", "--nest1", "N1");
            Assert.Equal("G", globalOptionValue);
            Assert.Equal("N1", nest1OptionValue);
            Assert.Equal("N2", nest2OptionValue);
        }

        [Fact]
        public void HelpTextIgnoresHiddenItems()
        {
            var app = new CommandLineApplication()
            {
                Name = "ninja-app",
                Description = "You can't see it until it is too late"
            };

            app.Command("star", c =>
            {
                c.Option("--points <p>", "How many", CommandOptionType.MultipleValue);
                c.ShowInHelpText = false;
            });
            app.Option("--smile", "Be a nice ninja", CommandOptionType.NoValue, o => { o.ShowInHelpText = false; });

            var a = app.Argument("name", "Pseudonym, of course");
            a.ShowInHelpText = false;

            var help = app.GetHelpText();

            Assert.Contains("ninja-app", help);
            Assert.DoesNotContain("--points", help);
            Assert.DoesNotContain("--smile", help);
            Assert.DoesNotContain("name", help);
        }

        [Fact]
        public void HelpTextUsesHelpOptionName()
        {
            var app = new CommandLineApplication
            {
                Name = "superhombre"
            };

            app.HelpOption("--ayuda-me");
            var help = app.GetHelpText();
            Assert.Contains("--ayuda-me", help);
        }
    }
}
