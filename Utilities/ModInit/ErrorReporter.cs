using BepInEx;
using BepInEx.Bootstrap;
using Menu;
using Mono.Cecil;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities.ModInit {
	public class ErrorReporter {

		private static bool _hasErrors = false;
		private static bool _reportedErrors = false;
		private static int _nextOrdinal = 0;
		private static readonly Dictionary<BaseUnityPlugin, ErrorReporter> _reporters = new Dictionary<BaseUnityPlugin, ErrorReporter>();
		private readonly Stack<Exception> _errors = new Stack<Exception>();
		private readonly Stack<string> _messages = new Stack<string>();

		/// <summary>
		/// If true, the loading sequence has already completed and thus it is too late to use a popup to report errors.
		/// <para/>
		/// Calling <see cref="DeferredReportModInitError(Exception, string)"/> will do nothing if this is true.
		/// </summary>
		public static bool IsTooLateToReport { get; private set; } = false;

		/// <summary>
		/// The name of the BepInEx mod that created this reporter.
		/// </summary>
		public string PluginName { get; }

		/// <summary>
		/// The instance of <see cref="BaseUnityPlugin"/> that instantiated this.
		/// </summary>
		public BaseUnityPlugin Plugin { get; }

		/// <summary>
		/// An ordinal number for this reporter.
		/// </summary>
		public int Ordinal { get; }

		internal static void Initialize() {
			Log.LogDebug("Injected error report hook...");
			//On.Menu.InitializationScreen.AfterModApplyingActions += InjectForErrorReporting;
			On.Menu.InitializationScreen.Update += InjectForErrorReporting;
		}

		private static void InjectForErrorReporting(On.Menu.InitializationScreen.orig_Update originalMethod, InitializationScreen @this) {
			IsTooLateToReport = true;
			if (_hasErrors && !_reportedErrors) {
				_reportedErrors = true;
				Log.LogDebug("One or more errors were reported to XansTools during loading. Displaying...");
				const string HEAD = "One or more errors have occurred during mods loaded with the help of XansTools! The mod(s) will be disabled the next time you run the game.\n\nA detailed log has been saved to a new folder named \"XansToolsReports\" in the Rain World directory. Please use this log to send to the appropriate mod developer(s).\n\n";
				string result = HEAD;
				foreach (ErrorReporter reporter in _reporters.Values) {
					foreach (Exception err in reporter._errors) {
						result += $"[{err.GetType().FullName}]: {err.Message}\n";
					}
					CrashReporter.ReportExceptions(reporter);
				}

				Vector2 screenSize = @this.manager.rainWorld.options.ScreenSize;
				Vector2 size = new Vector2(880f, 620f);
				@this.requiresRestartDialog = new DialogBoxNotify(@this, @this.pages[0], result, "RESTART", new Vector2(screenSize.x * 0.5f - (size.x * 0.5f), screenSize.y * 0.5f - (size.y * 0.5f)), size, true);
				@this.pages[0].subObjects.Add(@this.requiresRestartDialog);
				@this.currentStep = InitializationScreen.InitializationStep.REQUIRE_RESTART;
			} else {
				originalMethod(@this);
			}
		}

		public ErrorReporter(BaseUnityPlugin mod) {
			if (mod == null) throw new ArgumentNullException(nameof(mod));
			if (mod.GetType().Assembly != Assembly.GetCallingAssembly()) {
				string msg = "The mod type provided must be *your* type. Not someone else's type.\n";
				msg += $"Mod assembly: {mod.GetType().Assembly}\nCalling assembly: {Assembly.GetCallingAssembly()}";
				throw new ArgumentException(msg);
			}
			PluginName = mod.Info.Metadata.Name;
			Plugin = mod;
			Ordinal = _nextOrdinal++;
			_reporters[mod] = this;
		}

		/// <summary>
		/// Shows a popup reporting an error, with the intent of showing it after mods load. It will display when the popup is allowed to show.
		/// </summary>
		/// <param name="cause">The exception representing the error.</param>
		/// <param name="context">An optional message including additional context for why/where the error occurred.</param>
		public void DeferredReportModInitError(Exception cause, string context = null) {
			if (IsTooLateToReport) {
				Log.LogWarning($"Mod {PluginName} tried to call {nameof(DeferredReportModInitError)} but it's too late to do that, loading has already completed!");
				return;
			}

			Log.LogDebug($"An error was reported by {PluginName}");
			_errors.Push(cause);
			_messages.Push(context ?? string.Empty);
			_hasErrors = true;
		}


		/// <summary>
		/// The crash reporter from my SDL game I develop.
		/// </summary>
		private static class CrashReporter {

			private static void WriteStackTrace(IndentedTextWriter writer, string stackTrace) {
				if (stackTrace != null) {
					string[] lines = stackTrace.Split('\n');
					foreach (string line in lines) {
						writer.WriteLine(line.Trim());
					}
				}
			}

			private static void WriteAggregateException(IndentedTextWriter writer, AggregateException errs) {
				writer.WriteLine("[Multiple errors...]");
				writer.WriteLine("Caused by...");
				writer.Indent++;
				foreach (Exception err in errs.InnerExceptions) {
					WriteException(writer, err);
				}
				writer.Indent--;
			}

			private static void WriteException(IndentedTextWriter writer, Exception err) {
				if (err is AggregateException agg) {
					WriteAggregateException(writer, agg);
					return;
				}
				writer.WriteLine($"[{err.GetType().FullName}]: {err.Message}");
				writer.Indent++;
				WriteStackTrace(writer, err.StackTrace);
				if (err.InnerException != null) {
					writer.WriteLine();
					writer.WriteLine("Caused by...");
					WriteException(writer, err.InnerException);
				}
				writer.Indent--;
			}

			internal static void ReportExceptions(ErrorReporter reporter) {
				/*
				if (cause is SDLException sdlError) {
					if (string.IsNullOrWhiteSpace(context)) {
						context = sdlError.Context;
					}
				} else {
					if (string.IsNullOrWhiteSpace(context)) {
						context = "No additional context.";
					}
				}
				_log.LogFatal($"A critical error has occurred: {cause.Message}");
				*/
				IndentedTextWriter writer = null;
				try {
					DirectoryInfo logs = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "../..", "XansToolsReports"));
					if (!logs.Exists) logs.Create();

					writer = new IndentedTextWriter(new StreamWriter(File.OpenWrite(Path.Combine(logs.FullName, $"CRASH {GetFileFriendlyTimeName()}-{reporter.Ordinal:D4}.log"))));
					writer.WriteLine($"https://youtu.be/cI2JpMCpl3I?t=290\n\nDate: {DateTime.UtcNow.ToLongDateString()} (UTC)\nTime: {DateTime.UtcNow.ToLongTimeString()} (UTC)\nMod: {reporter.PluginName}\n\nBEGIN REPORT ::");

					int amount = reporter._errors.Count;
					for (int i = 0; i < amount; i++) {
						Exception error = reporter._errors.Pop();
						string context = reporter._messages.Pop();
						if (string.IsNullOrWhiteSpace(context)) {
							context = "No additional context was provided.";
						}

						writer.Indent++;
						writer.WriteLine($"ERROR NO. {i+1}");
						writer.WriteLine($"CONTEXT: {context}");
						writer.Indent++;
						WriteException(writer, error);
						writer.Indent--;
						writer.Indent--;
					}
				} catch (Exception ex) {
					//_log.LogFatal($"...But an error also occurred while trying to save the log file (nice): {ex.Message}");
					Log.LogFatal($"...an error also occurred while trying to save the log file for the first error (nice): {ex.Message}");
				} finally {
					if (writer != null) {
						writer.Flush();
						writer.Close();
						writer.Dispose();
					}
				}

			}
			private static string GetFileFriendlyTimeName() {
				DateTimeOffset now = DateTimeOffset.Now;
				return $"{now.Day:D2}-{now.Month:D2}-{now.Year:D4} at {now.Hour:D2}.{now.Minute:D2}";
			}
		}

	}
}
