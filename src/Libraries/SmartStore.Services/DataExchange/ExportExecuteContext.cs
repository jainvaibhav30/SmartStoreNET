﻿using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange
{
	public interface IExportExecuteContext
	{
		/// <summary>
		/// Provides the data to be exported
		/// </summary>
		IExportSegmenter Data { get; }

		/// <summary>
		/// The store context to be used for the export
		/// </summary>
		ExpandoObject Store { get; }

		/// <summary>
		/// The customer context to be used for the export
		/// </summary>
		ExpandoObject Customer { get; }

		/// <summary>
		/// The currency context to be used for the export
		/// </summary>
		ExpandoObject Currency { get; }

		/// <summary>
		/// The language identifier to be used for the export. Can be 0.
		/// </summary>
		int LanguageId { get; }

		/// <summary>
		/// To log information into the export log file
		/// </summary>
		ILogger Log { get; }

		/// <summary>
		/// Whether the export has been cancelled
		/// </summary>
		bool IsCanceled { get; }

		/// <summary>
		/// The maximum allowed file name length
		/// </summary>
		int MaxFileNameLength { get; }

		/// <summary>
		/// The path of the export folder
		/// </summary>
		string Folder { get; }

		/// <summary>
		/// The name of the current export file
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// The path of the current export file
		/// </summary>
		string FilePath { get; }

		/// <summary>
		/// Provider specific configuration data
		/// </summary>
		object ConfigurationData { get; }

		/// <summary>
		/// Use this dictionary for any custom data required along the export
		/// </summary>
		Dictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// Number of successful exported records. Should be incremented by the provider. Will be logged.
		/// </summary>
		int SuccessfulExportedRecords { get; set; }
	}


	public class ExportExecuteContext : IExportExecuteContext
	{
		private CancellationToken _cancellation;
		private IExportSegmenter _segmenter;

		internal ExportExecuteContext(CancellationToken cancellation, string folder)
		{
			_cancellation = cancellation;
			Folder = folder;

			CustomProperties = new Dictionary<string, object>();
		}

		public IExportSegmenter Data
		{
			get
			{
				return _segmenter;
			}
			internal set
			{
				if (_segmenter != null)
					(_segmenter as IExportExecuter).Dispose();

				_segmenter = value;
			}
		}

		public ExpandoObject Store { get; internal set; }
		public ExpandoObject Customer { get; internal set; }
		public ExpandoObject Currency { get; internal set; }
		public int LanguageId { get; internal set; }

		public ILogger Log { get; internal set; }

		public bool IsCanceled
		{
			get { return _cancellation.IsCancellationRequested; }
		}

		public int MaxFileNameLength { get; internal set; }

		public string Folder { get; private set; }
		public string FileNamePattern { get; internal set; }
		public string FileExtension { get; internal set; }
		public string FileName
		{
			get
			{
				var finallyResolvedPattern = FileNamePattern
					.Replace("%Misc.FileNumber%", (Data.FileIndex + 1).ToString("D5"))
					.ToValidFileName("")
					.Truncate(MaxFileNameLength);

				return string.Concat(finallyResolvedPattern, FileExtension);
			}
		}
		public string FilePath
		{
			get { return Path.Combine(Folder, FileName); }
		}

		public object ConfigurationData { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public int SuccessfulExportedRecords { get; set; }
	}
}