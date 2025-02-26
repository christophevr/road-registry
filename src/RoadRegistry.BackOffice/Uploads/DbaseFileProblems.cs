namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;

    public static class DbaseFileProblems
    {
        // file

        public static FileProblem HasNoDbaseRecords(this IFileProblemBuilder builder, bool treatAsWarning)
        {
            if (treatAsWarning)
                return builder.Warning(nameof(HasNoDbaseRecords)).Build();
            return builder.Error(nameof(HasNoDbaseRecords)).Build();
        }

        public static FileProblem HasTooManyDbaseRecords(this IFileProblemBuilder builder, int expectedCount, int actualCount)
        {
            return builder
                .Error(nameof(HasNoDbaseRecords))
                .WithParameters(
                    new ProblemParameter("ExpectedCount",expectedCount.ToString()),
                    new ProblemParameter("ActualCount",actualCount.ToString()))
                .Build();
        }

        public static FileError HasDbaseHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(HasDbaseHeaderFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError HasDbaseSchemaMismatch(this IFileProblemBuilder builder, DbaseSchema expectedSchema, DbaseSchema actualSchema)
        {
            if (expectedSchema == null) throw new ArgumentNullException(nameof(expectedSchema));
            if (actualSchema == null) throw new ArgumentNullException(nameof(actualSchema));

            return builder
                .Error(nameof(HasDbaseSchemaMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedSchema", Describe(expectedSchema)),
                    new ProblemParameter("ActualSchema", Describe(actualSchema))
                )
                .Build();
        }

        private static string Describe(DbaseSchema schema)
        {
            var builder = new StringBuilder();
            var index = 0;
            foreach (var field in schema.Fields)
            {
                if (index > 0) builder.Append(", ");
                builder.Append(field.Name.ToString());
                builder.Append("[");
                builder.Append(field.FieldType.ToString());
                builder.Append("(");
                builder.Append(field.Length.ToString());
                builder.Append(",");
                builder.Append(field.DecimalCount.ToString());
                builder.Append(")");
                builder.Append("]");
                index++;
            }

            return builder.ToString();
        }

        public static FileError RoadSegmentsWithoutLaneAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            return builder
                .Error(nameof(RoadSegmentsWithoutLaneAttributes))
                .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
                .Build();
        }

        public static FileError RoadSegmentsWithoutSurfaceAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            return builder
                .Error(nameof(RoadSegmentsWithoutSurfaceAttributes))
                .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
                .Build();
        }

        public static FileError RoadSegmentsWithoutWidthAttributes(this IFileProblemBuilder builder, RoadSegmentId[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            return builder
                .Error(nameof(RoadSegmentsWithoutWidthAttributes))
                .WithParameter(new ProblemParameter("Segments", string.Join(",", segments.Select(segment => segment.ToString()))))
                .Build();
        }

        // record

        public static FileError HasDbaseRecordFormatError(this IDbaseFileRecordProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(HasDbaseRecordFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError IdentifierZero(this IDbaseFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(IdentifierZero)).Build();
        }

        public static FileError RequiredFieldIsNull(this IDbaseFileRecordProblemBuilder builder, DbaseField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            return builder
                .Error(nameof(RequiredFieldIsNull))
                .WithParameter(new ProblemParameter("Field", field.Name.ToString()))
                .Build();
        }

        public static FileError DownloadIdInvalidFormat(this IDbaseFileRecordProblemBuilder builder, string value)
        {
            return builder
                .Error(nameof(DownloadIdInvalidFormat))
                .WithParameter(new ProblemParameter("Actual", value))
                .Build();
        }

        public static FileError DownloadIdDiffersFromMetadata(this IDbaseFileRecordProblemBuilder builder, string value, string expectedValue)
        {
            return builder
                .Error(nameof(DownloadIdDiffersFromMetadata))
                .WithParameter(new ProblemParameter("Actual", value))
                .WithParameter(new ProblemParameter("Expected", expectedValue))
                .Build();
        }

        public static FileError OrganizationIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, string value)
        {
            return builder
                .Error(nameof(OrganizationIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value))
                .Build();
        }

        public static FileError RoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(RoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError RoadSegmentMissing(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(RoadSegmentMissing))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        private static readonly NumberFormatInfo Provider = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        public static FileError FromPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(FromPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError ToPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(ToPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError FromPositionEqualToOrGreaterThanToPosition(this IDbaseFileRecordProblemBuilder builder,
            double from, double to)
        {
            return builder
                .Error(nameof(FromPositionEqualToOrGreaterThanToPosition))
                .WithParameter(new ProblemParameter("From", from.ToString(Provider)))
                .WithParameter(new ProblemParameter("To", to.ToString(Provider)))
                .Build();
        }

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            AttributeId identifier,
            RecordNumber takenByRecordNumber)
        {
            return builder
                .Error(nameof(IdentifierNotUnique))
                .WithParameters(
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()))
                .Build();
        }

        // european road

        public static FileError NotEuropeanRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotEuropeanRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        // grade separated junction

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            GradeSeparatedJunctionId identifier,
            RecordNumber takenByRecordNumber)
        {
            return builder
                .Error(nameof(IdentifierNotUnique))
                .WithParameters(
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
                .Build();
        }

        public static FileError GradeSeparatedJunctionTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(GradeSeparatedJunctionTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", GradeSeparatedJunctionType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError UpperRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(UpperRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError LowerRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(LowerRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        // national road

        public static FileError NotNationalRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNationalRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        // numbered road

        public static FileError NotNumberedRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNumberedRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        public static FileError NumberedRoadOrdinalOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(NumberedRoadOrdinalOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError NumberedRoadDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(NumberedRoadDirectionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentNumberedRoadDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // record type

        public static FileError RecordTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RecordTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RecordType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RecordTypeNotSupported(this IDbaseFileRecordProblemBuilder builder, int actual, params int[] expected)
        {
            return builder
                .Error(nameof(RecordTypeNotSupported))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", expected.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // road node

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            RoadNodeId identifier,
            RecordNumber takenByRecordNumber)
        {
            return builder
                .Error(nameof(IdentifierNotUnique))
                .WithParameters(
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
                .Build();
        }

        public static FileWarning IdentifierNotUniqueButAllowed(this IDbaseFileRecordProblemBuilder builder,
            RoadNodeId identifier,
            RecordType recordType,
            RecordNumber takenByRecordNumber,
            RecordType takenByRecordType)
        {
            return builder
                .Warning(nameof(IdentifierNotUniqueButAllowed))
                .WithParameters(
                    new ProblemParameter("RecordType", recordType.ToString()),
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString()),
                    new ProblemParameter("TakenByRecordType", takenByRecordType.ToString())
                )
                .Build();
        }

        public static FileError RoadNodeTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadNodeTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadNodeType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // road segment

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            RoadSegmentId identifier,
            RecordNumber takenByRecordNumber)
        {
            return builder
                .Error(nameof(IdentifierNotUnique))
                .WithParameters(
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
                .Build();
        }

        public static FileError RoadSegmentAccessRestrictionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentAccessRestrictionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentAccessRestriction.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentStatusMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentStatusMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentStatus.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentCategoryMismatch(this IDbaseFileRecordProblemBuilder builder, string actual)
        {
            return builder
                .Error(nameof(RoadSegmentCategoryMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentCategory.ByIdentifier.Keys.Select(key => key))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual))
                .Build();
        }

        public static FileError RoadSegmentGeometryDrawMethodMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentGeometryDrawMethodMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentGeometryDrawMethod.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentMorphologyMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentMorphologyMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentMorphology.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError BeginRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(BeginRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError EndRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(EndRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError LeftStreetNameIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(LeftStreetNameIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError RightStreetNameIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(RightStreetNameIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError BeginRoadNodeIdEqualsEndRoadNode(this IDbaseFileRecordProblemBuilder builder,
            int beginNode,
            int endNode)
        {
            return builder
                .Error(nameof(BeginRoadNodeIdEqualsEndRoadNode))
                .WithParameter(new ProblemParameter("Begin", beginNode.ToString()))
                .WithParameter(new ProblemParameter("End", endNode.ToString()))
                .Build();
        }

        // lane

        public static FileError LaneCountOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(LaneCountOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }

        public static FileError LaneDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(LaneDirectionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentLaneDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // surface

        public static FileError SurfaceTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(SurfaceTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentSurfaceType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // width

        public static FileError WidthOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(WidthOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }
    }
}
