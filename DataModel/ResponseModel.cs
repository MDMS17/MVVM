using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace mcpdipData
{
    public class McpdipHeader
    {
        [Key]
        public int HeaderId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string SubmitterName { get; set; }
        public string SubmissionDate { get; set; }
        public string ValidationStatus { get; set; }
        public string Levels { get; set; }
        public string SchemaVersion { get; set; }
        public string RecordYear { get; set; }
        public string RecordMonth { get; set; }
    }
    public class McpdipHierarchy
    {
        [Key]
        public long HierarchyId { get; set; }
        public long HeaderId { get; set; }
        public string LevelIdentifier { get; set; }
        public string SectionIdentifier { get; set; }
    }
    public class McpdipChildren
    {
        [Key]
        public long ChildrenId { get; set; }
        public long HierarchyId { get; set; }
        public string LevelIdentifier { get; set; }
        public string SectionIDentifier { get; set; }
    }

    public class McpdipDetail
    {
        [Key]
        public long DetailId { get; set; }
        public string ResponseTarget { get; set; }
        public long ChildrenId { get; set; }
        public string ItemReference { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string OriginalTable { get; set; }
        public long? OriginalId { get; set; }
        public long? OriginalHeaderId { get; set; }
        public string OriginalCin { get; set; }
        public string OriginalItemId { get; set; }
    }

    public class ResponseFile
    {
        public string fileName { get; set; }
        public string fileType { get; set; }
        public string submitterName { get; set; }
        public string submissionDate { get; set; }
        public string validationStatus { get; set; }
        public string schemaVersion { get; set; }
        public string levels { get; set; }
        public List<ResponseHierarchy> responseHierarchy { get; set; }
    }
    public class ResponseDetail
    {
        public string itemReference { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string severity { get; set; }
    }
    public class ResponseHierarchy
    {
        public string levelIdentifier { get; set; }
        public string sectionIdentifier { get; set; }
        public List<ResponseDetail> responses { get; set; }
        public List<ResponseChildren> children { get; set; }
    }
    public class ResponseChildren
    {
        public string levelIdentifier { get; set; }
        public string sectionIdentifier { get; set; }
        public List<ResponseDetail> responses { get; set; }
    }
}

