namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;
    using Schema.V2;
    using Xunit;

    public class TransactionZoneDbaseRecordsValidatorTests : IDisposable
    {
        private readonly TransactionZoneDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<TransactionZoneDbaseRecord> _enumerator;
        private readonly ZipArchiveValidationContext _context;

        public TransactionZoneDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeReason();
            _fixture.CustomizeOperatorName();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeDownloadId();
            _fixture.Customize<TransactionZoneDbaseRecord>(
                composer => composer
                    .FromFactory(random => new TransactionZoneDbaseRecord
                    {
                        SOURCEID = {Value = random.Next(1, 5)},
                        TYPE = {Value = random.Next(1, 9999)},
                        BESCHRIJV = { Value = _fixture.Create<Reason>().ToString() },
                        OPERATOR = { Value = _fixture.Create<OperatorName>().ToString() },
                        ORG = { Value = _fixture.Create<OrganizationId>().ToString() },
                        APPLICATIE =
                        {
                            Value = new string(_fixture
                                .CreateMany<char>(TransactionZoneDbaseRecord.Schema.APPLICATIE.Length.ToInt32())
                                .ToArray())
                        },
                        DOWNLOADID = { Value = _fixture.Create<DownloadId>().ToString() }
                    })
                    .OmitAutoProperties());

            _sut = new TransactionZoneDbaseRecordsValidator();
            _enumerator = new List<TransactionZoneDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("transactiezone.dbf");
            _context = ZipArchiveValidationContext.Empty;
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<TransactionZoneDbaseRecord>>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator, _context));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null, _context));
        }

        [Fact]
        public void ValidateContextCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, _enumerator, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var (result, context) = _sut.Validate(_entry, _enumerator, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(false)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithMoreThanOneRecordReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(2)
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasTooManyDbaseRecords(1, 2)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingReasonReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.BESCHRIJV.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.BESCHRIJV)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingOperatorNameReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.OPERATOR.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.OPERATOR)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingOrganizationIdReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.ORG.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.ORG)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatHasOrganizationIdOutOfRangeReturnsExpectedResult()
        {
            var value = string.Empty;
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.ORG.Value = value;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).OrganizationIdOutOfRange(value)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingDownloadIdReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.DOWNLOADID.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.DOWNLOADID)),
                result);
            Assert.Same(_context, context);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("not a numerical guid")]
        public void ValidateWithRecordThatIsInvalidDownloadIdReturnsExpectedResult(string invalidDownloadId)
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.DOWNLOADID.Value = invalidDownloadId;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).DownloadIdInvalidFormat(invalidDownloadId)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveEmptyStringAsRequiredFieldValueReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.BESCHRIJV.Value = string.Empty;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.BESCHRIJV)),
                result);
            Assert.Same(_context, context);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
