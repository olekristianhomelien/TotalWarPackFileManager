using Filetypes.ByteParsing;
using Filetypes.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Filetypes {
	/*
	 * Class representing the first bytes in a db file;
	 * these contain information about db version, maybe an id,
	 * and a count of contained entries.
	 */
    public class DBFileHeader {
        public string GUID { get; set; }
        public bool HasVersionMarker { get; set; }
        public int Version { get; set; }
        public uint EntryCount { get; set; }
        public byte UnknownByte { get; set; }
        /*
         * The length of the encoded header.
         */
		public int Length {
			get {
				int result = 5;
				result += (GUID.Length != 0) ? 78 : 0;
				result += HasVersionMarker ? 8 : 0;
				return result;
			}
		}
        /*
         * Create header with the given GUID, version and entry count.
         */
        public DBFileHeader() 
        {
        }
        
		#region Framework Overrides
        public override bool Equals(object other) {
            bool result = false;
            if (other is DBFileHeader) {
                DBFileHeader header2 = (DBFileHeader)other;
                result = GUID.Equals(header2.GUID);
                result &= Version.Equals(header2.Version);
                result &= EntryCount.Equals(header2.EntryCount);
            }
            return result;
        }
        public override int GetHashCode() {
            return GUID.GetHashCode();
        }
		#endregion
            }

	/*
	 * Class representing a database file.
	 */
    public class DBFile {
        private List<DBRow> _entries = new List<DBRow>();
        public DBFileHeader Header { get; private set; }
        public DbTableDefinition CurrentType {
            get;
            set;
        }

		#region Attributes
		// the entries of this file
        public List<DBRow> Entries {
            get {
                return this._entries;
            }
        }

        // access by row/column
		public DbField this [int row, int column] {
			get {
				return _entries [row][column];
			}
		}
		#endregion

        #region Constructors
        /*
         * Create db file with the given header and the given type.
         */
        public DBFile (DBFileHeader h, DbTableDefinition info) {
			Header = h;
			CurrentType = info;
		}
        #endregion

        /**
         * <summary>Checks whether this DBFile contains a <see cref="DBRow"/> with equivalent data to a given DBRow.</summary>
         * 
         * <param name="checkRow">The <see cref="DBRow"/> to be compared to this DBFile's rows.</param>
         * <returns>Whether the DBFile contains a DBRow with equivalent data to <paramref name="checkRow"/>.</returns>
         */
        public bool ContainsRow(DBRow checkRow)
        {
            foreach(DBRow row in Entries)
                if(row.SameData(checkRow))
                    return true;
            return false;
        }


        /*
         * Helper to retrieve type name from a file path.
         */
		public static string Typename(string fullPath) {
			return Path.GetFileName(Path.GetDirectoryName (fullPath));
		}
	}

    public class DbColumnDefinition : ICloneable
    {
        public string Name { get; set; }
        public string FieldReference { get; set; }
        public string TableReference { get; set; }
        public bool IsKey { get; set; } = false;
        public bool IsOptional { get; set; }
        public int MaxLength { get; set; }
        public bool IsFileName { get; set; } = false;
        public string Description { get; set; }
        public string FilenameRelativePath { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        public DbTypesEnum Type { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class DbField : ICloneable
    {
        IByteParser _parser;
        string _error = "";
        string _value = "";

        public DbField(DbTypesEnum type)
        {
            Type = type;
        }

        public IByteParser Parser { get { return _parser; } }
        public string Value { get { return _value; } set { _value = value; } }
        public string Error { get { return _error; } }
        public bool HasError { get { return !string.IsNullOrWhiteSpace(_error); } }
        public DbTypesEnum Type { get { return _parser.Type; } set { _parser = ByteParserFactory.Create(value); } }
        public void Decode(ByteChunk chunk)
        {
            chunk.Read(_parser, out _value, out _error);
        }

        public void Encode(BinaryWriter writer)
        { }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class DBRow : List<DbField>
    {
        private DbTableDefinition info;

        public DBRow(DbTableDefinition i, List<DbField> val) : base(val)
        {
            info = i;
        }
        public DBRow(DbTableDefinition i) : this(i, CreateRow(i)) { }


        public DbField this[string fieldName]
        {
            get
            {
                return this[IndexOfField(fieldName)];
            }
            set
            {
                this[IndexOfField(fieldName)] = value;
            }
        }

        public bool SameData(DBRow row)
        {
            if (Count != row.Count)
                return false;
            for (int i = 0; i < Count; ++i)
                if (!this[i].Value.Equals(row[i].Value))
                    return false;
            return true;
        }

        private int IndexOfField(string fieldName)
        {
            for (int i = 0; i < info.ColumnDefinitions.Count; i++)
            {
                if (info.ColumnDefinitions[i].Name == fieldName)
                    return i;
            }
            throw new IndexOutOfRangeException(string.Format("Field name {0} not valid for type {1}", fieldName, info.TableName));
        }

        static List<DbField> CreateRow(DbTableDefinition info)
        {
            List<DbField> result = new List<DbField>(info.ColumnDefinitions.Count);
            foreach (var item in info.ColumnDefinitions)
            {
                var dbfield = new DbField(item.Type);
                result.Add(dbfield);
            }
            return result;
        }
    }
}
