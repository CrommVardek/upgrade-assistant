﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AspNetMigrator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingSystemWebAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AM0001";
        private const string Category = "Migration";
        private static readonly string[] DisallowedNamespaces = new[] { "System.Web", "Microsoft.AspNet", "Microsoft.Owin", "Owin" };

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.UsingSystemWebTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.UsingSystemWebMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.UsingSystemWebDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            context.RegisterSyntaxNodeAction(AnalyzeUsingStatements, SyntaxKind.UsingDirective);
        }

        private void AnalyzeUsingStatements(SyntaxNodeAnalysisContext context)
        {
            var usingDirective = context.Node as UsingDirectiveSyntax;
            var namespaceName = usingDirective?.Name?.ToString();

            if (namespaceName is null)
            {
                return;
            }

            if (DisallowedNamespaces.Any(name => namespaceName.Equals(name, StringComparison.Ordinal) || namespaceName.StartsWith($"{name}.", StringComparison.Ordinal)))
            {
                var diagnostic = Diagnostic.Create(Rule, usingDirective.GetLocation(), namespaceName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}