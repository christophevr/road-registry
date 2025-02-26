namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryValidator<TDbaseRecord> : IZipArchiveDbaseEntryValidator
        where TDbaseRecord : DbaseRecord, new()
    {
        private readonly IZipArchiveDbaseRecordsValidator<TDbaseRecord> _recordValidator;

        public Encoding Encoding { get; }
        public DbaseSchema Schema { get; }
        public DbaseFileHeaderReadBehavior HeaderReadBehavior { get; }

        public ZipArchiveDbaseEntryValidator(
            Encoding encoding,
            DbaseFileHeaderReadBehavior readBehavior,
            DbaseSchema schema,
            IZipArchiveDbaseRecordsValidator<TDbaseRecord> recordValidator)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            HeaderReadBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = ZipArchiveProblems.None;
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding))
            {
                DbaseFileHeader header = null;
                try
                {
                    header = DbaseFileHeader.Read(reader, HeaderReadBehavior);
                }
                catch (Exception exception)
                {
                    problems += entry.HasDbaseHeaderFormatError(exception);
                }

                if (header != null)
                {
                    if (!header.Schema.Equals(Schema))
                    {
                        problems += entry.HasDbaseSchemaMismatch(Schema, header.Schema);
                    }
                    else
                    {
                        using (var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
                        {
                            var (recordProblems, recordContext) = _recordValidator.Validate(entry, records, context);
                            problems += recordProblems;
                            context = recordContext;
                        }
                    }
                }
            }

            return (problems, context);
        }
    }
}
