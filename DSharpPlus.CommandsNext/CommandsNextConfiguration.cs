﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// <para>Represents a delegate for a function that takes a message, and returns the position of the start of command invocation in the message. It has to return -1 if prefix is not present.</para>
    /// <para>
    /// It is recommended that helper methods <see cref="CommandsNextUtilities.GetStringPrefixLength(DiscordMessage, string)"/> and <see cref="CommandsNextUtilities.GetMentionPrefixLength(DiscordMessage, DiscordUser)"/>
    /// be used internally for checking. Their output can be passed through.
    /// </para>
    /// </summary>
    /// <param name="msg">Message to check for prefix.</param>
    /// <returns>Position of the command invocation or -1 if not present.</returns>
    public delegate Task<int> PrefixResolverDelegate(DiscordMessage msg);

    /// <summary>
    /// Represents a configuration for <see cref="CommandsNextExtension"/>.
    /// </summary>
    public sealed class CommandsNextConfiguration
    {
        /// <summary>
        /// <para>Sets the string prefixes used for commands.</para>
        /// <para>Defaults to no value (disabled).</para>
        /// </summary>
        public IEnumerable<string> StringPrefixes { internal get; set; }

        /// <summary>
        /// <para>Sets the custom prefix resolver used for commands.</para>
        /// <para>Defaults to none (disabled).</para>
        /// </summary>
        public PrefixResolverDelegate PrefixResolver { internal get; set; } = null;

        /// <summary>
        /// <para>Sets whether to allow mentioning the bot to be used as command prefix.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableMentionPrefix { internal get; set; } = true;

        /// <summary>
        /// <para>Sets whether the bot should only respond to messages from its own account. This is useful for selfbots.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool Selfbot { internal get; set; } = false;

        /// <summary>
        /// <para>Sets whether the commands should be case-sensitive.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool CaseSensitive { internal get; set; } = false;

        /// <summary>
        /// <para>Sets whether to enable default help command.</para>
        /// <para>Disabling this will allow you to make your own help command.</para>
        /// <para>
        /// Modifying default help can be achieved via custom help formatters (see <see cref="BaseHelpFormatter"/> and <see cref="CommandsNextExtension.SetHelpFormatter{T}()"/> for more details). 
        /// It is recommended to use help formatter instead of disabling help.
        /// </para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableDefaultHelp { internal get; set; } = true;

        /// <summary>
        /// <para>Sets the default pre-execution checks for the built-in help command.</para>
        /// <para>Only applicable if default help is enabled.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IEnumerable<CheckBaseAttribute> DefaultHelpChecks { internal get; set; } = null;

        /// <summary>
        /// <para>Sets whether commands sent via direct messages should be processed.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableDms { internal get; set; } = true;

        /// <summary>
        /// <para>Sets the service provider for this CommandsNext instance.</para>
        /// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { internal get; set; } = null;

        /// <summary>
        /// <para>Gets whether any extra arguments passed to commands should be ignored or not. If this is set to false, extra arguments will throw, otherwise they will be ignored.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool IgnoreExtraArguments { internal get; set; } = false;

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>.
        /// </summary>
        public CommandsNextConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public CommandsNextConfiguration(CommandsNextConfiguration other)
        {
            this.CaseSensitive = other.CaseSensitive;
            this.PrefixResolver = other.PrefixResolver;
            this.DefaultHelpChecks = other.DefaultHelpChecks;
            this.EnableDefaultHelp = other.EnableDefaultHelp;
            this.EnableDms = other.EnableDms;
            this.EnableMentionPrefix = other.EnableMentionPrefix;
            this.IgnoreExtraArguments = other.IgnoreExtraArguments;
            this.Selfbot = other.Selfbot;
            this.Services = other.Services;
            this.StringPrefixes = other.StringPrefixes?.ToArray();
        }
    }
}
