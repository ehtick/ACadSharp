﻿using ACadSharp.Header;
using ACadSharp.IO;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public abstract class CadReaderTestsBase<T> : IOTestsBase, IDisposable
		where T : ICadReader
	{
		protected readonly Dictionary<string, CadDocument> _documents = new Dictionary<string, CadDocument>();  //TODO: this does not store the document readed

		public CadReaderTestsBase(ITestOutputHelper output) : base(output)
		{
		}

		public virtual void ReadHeaderTest(string test)
		{
			using (T reader = (T)Activator.CreateInstance(typeof(T), test, null))
			{
				reader.OnNotification += this.onNotification;
				CadHeader header = reader.ReadHeader();
			}
		}

		public virtual void ReadTest(string test)
		{
			CadDocument doc = this.getDocument(test);

			Assert.NotNull(doc);
		}

		public virtual void AssertDocumentDefaults(string test)
		{
			CadDocument doc = this.getDocument(test);

			Assert.NotNull(doc.SummaryInfo);

			if (doc.Header.Version < ACadVersion.AC1012)
			{
				//Older version do not keep the handles for tables and other objects like block_records
				//This can be fixed if the document creates the default entries manually
				return;
			}

			this._docIntegrity.AssertDocumentDefaults(doc);
		}

		public virtual void AssertTableHirearchy(string test)
		{
			CadDocument doc = this.getDocument(test);

			this._docIntegrity.AssertTableHirearchy(doc);
		}

		public virtual void AssertBlockRecords(string test)
		{
			CadDocument doc = this.getDocument(test);

			this._docIntegrity.AssertBlockRecords(doc);
		}

		public virtual void AssertDocumentContent(string test)
		{
			CadDocument doc = this.getDocument(test, false);

			if (doc.Header.Version < ACadVersion.AC1012)
			{
				//Older version do not keep the handles for tables and other objects like block_records
				return;
			}

			this._docIntegrity.AssertDocumentContent(doc);
		}

		public virtual void AssertDocumentTree(string test)
		{
			CadDocument doc = this.getDocument(test, false);

			this._docIntegrity.AssertDocumentTree(doc);
		}

		public void Dispose()
		{
			this._documents.Clear();
		}

		protected CadDocument getDocument(string path, bool addEvent = true)
		{
			if (_documents.TryGetValue(path, out var doc))
				return doc;

			using (T reader = (T)Activator.CreateInstance(typeof(T), path, null))
			{
				if (addEvent)
				{
					reader.OnNotification += this.onNotification;
				}

				doc = reader.Read();
			}

			_documents.Add(path, doc);

			return doc;
		}
	}
}
