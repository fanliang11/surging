using DotNetty.Codecs.DNS.Records;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetty.Codecs.DNS.Messages
{
    /// <summary>
    /// Defines the <see cref="AbstractDnsMessage" />
    /// </summary>
    public class AbstractDnsMessage : AbstractReferenceCounted, IDnsMessage
    {
        #region 常量

        /// <summary>
        /// Defines the SECTION_COUNT
        /// </summary>
        private const int SECTION_COUNT = 4;

        /// <summary>
        /// Defines the SECTION_QUESTION
        /// </summary>
        private const DnsSection SECTION_QUESTION = DnsSection.QUESTION;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the leakDetector
        /// </summary>
        private static readonly ResourceLeakDetector leakDetector = ResourceLeakDetector.Create<IDnsMessage>();

        /// <summary>
        /// Defines the leak
        /// </summary>
        private readonly IResourceLeakTracker leak;

        /// <summary>
        /// Defines the additionals
        /// </summary>
        private object additionals;

        /// <summary>
        /// Defines the answers
        /// </summary>
        private object answers;

        /// <summary>
        /// Defines the authorities
        /// </summary>
        private object authorities;

        /// <summary>
        /// Defines the questions
        /// </summary>
        private object questions;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDnsMessage"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        protected AbstractDnsMessage(int id) : this(id, DnsOpCode.QUERY)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDnsMessage"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="opcode">The opcode<see cref="DnsOpCode"/></param>
        protected AbstractDnsMessage(int id, DnsOpCode opcode)
        {
            Id = id;
            OpCode = opcode;
            leak = leakDetector.Track(this);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsRecursionDesired
        /// </summary>
        public bool IsRecursionDesired { get; set; }

        /// <summary>
        /// Gets or sets the OpCode
        /// </summary>
        public DnsOpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the Z
        /// </summary>
        public int Z { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        public void AddRecord(DnsSection section, IDnsRecord record)
        {
            CheckQuestion(section, record);

            object records = SectionAt(section);
            if (records == null)
            {
                SetSection(section, record);
                return;
            }

            List<IDnsRecord> recordList;
            if (records is IDnsRecord)
            {
                recordList = new List<IDnsRecord>(2);
                recordList.Add((IDnsRecord)records);
                recordList.Add(record);
                SetSection(section, recordList);
                return;
            }

            recordList = (List<IDnsRecord>)records;
            recordList.Add(record);
        }

        /// <summary>
        /// The AddRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        public void AddRecord(DnsSection section, int index, IDnsRecord record)
        {
            CheckQuestion(section, record);

            object records = SectionAt(section);
            if (records == null)
            {
                if (index != 0)
                    throw new IndexOutOfRangeException($"index: {index} (expected: 0)");

                SetSection(section, record);
                return;
            }

            List<IDnsRecord> recordList;
            if (records is IDnsRecord)
            {
                if (index == 0)
                {
                    recordList = new List<IDnsRecord>();
                    recordList.Add(record);
                    recordList.Add((IDnsRecord)records);
                }
                else if (index == 1)
                {
                    recordList = new List<IDnsRecord>();
                    recordList.Add((IDnsRecord)records);
                    recordList.Add(record);
                }
                else
                {
                    throw new IndexOutOfRangeException($"index: {index} (expected: 0 or 1)");
                }
                SetSection(section, recordList);
                return;
            }

            recordList = (List<IDnsRecord>)records;
            recordList[index] = record;
        }

        /// <summary>
        /// The Clear
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < SECTION_COUNT; i++)
            {
                Clear((DnsSection)i);
            }
        }

        /// <summary>
        /// The Clear
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        public void Clear(DnsSection section)
        {
            object recordOrList = SectionAt(section);
            SetSection(section, null);

            if (recordOrList is IReferenceCounted)
            {
                ((IReferenceCounted)recordOrList).Release();
            }
            else if (recordOrList is IList)
            {
                List<IDnsRecord> list = (List<IDnsRecord>)recordOrList;
                if (list.Count == 0)
                {
                    foreach (var r in list)
                    {
                        ReferenceCountUtil.Release(r);
                    }
                }
            }
        }

        /// <summary>
        /// The Count
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int Count()
        {
            int count = 0;
            for (int i = 0; i < SECTION_COUNT; i++)
            {
                count += Count((DnsSection)i);
            }
            return count;
        }

        /// <summary>
        /// The Count
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="int"/></returns>
        public int Count(DnsSection section)
        {
            object records = SectionAt(section);
            if (records == null)
                return 0;

            if (records is IDnsRecord)
                return 1;

            List<IDnsRecord> recordList = (List<IDnsRecord>)records;
            return recordList.Count;
        }

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            if (this == obj) return true;

            if (!(obj is IDnsMessage)) return false;

            IDnsMessage that = (IDnsMessage)obj;
            if (Id != that.Id)
                return false;

            if (this is IDnsQuestion)
            {
                if (!(that is IDnsQuestion))
                    return false;
            }
            else if (that is IDnsQuestion)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return Id * 31 + (this is IDnsQuestion ? 0 : 1);
        }

        /// <summary>
        /// The GetRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="TRecord"/></returns>
        public TRecord GetRecord<TRecord>(DnsSection section) where TRecord : IDnsRecord
        {
            object records = SectionAt(section);
            if (records == null)
                return default(TRecord);

            if (records is IDnsRecord)
                return (TRecord)records;

            List<IDnsRecord> recordList = (List<IDnsRecord>)records;
            if (recordList.Count == 0)
                return default(TRecord);

            return (TRecord)recordList[0];
        }

        /// <summary>
        /// The GetRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <returns>The <see cref="TRecord"/></returns>
        public TRecord GetRecord<TRecord>(DnsSection section, int index) where TRecord : IDnsRecord
        {
            object records = SectionAt(section);
            if (records == null)
                throw new IndexOutOfRangeException($"index: {index} (expected: none)");

            if (records is IDnsRecord)
            {
                if (index == 0)
                    return (TRecord)records;

                throw new IndexOutOfRangeException($"index: {index} (expected: 0)");
            }

            List<IDnsRecord> recordList = (List<IDnsRecord>)records;
            return (TRecord)recordList[index];
        }

        /// <summary>
        /// The RemoveRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        public void RemoveRecord(DnsSection section, int index)
        {
            object records = SectionAt(section);
            if (records == null)
                throw new IndexOutOfRangeException($"index: {index} (expected: none)");

            if (records is IDnsRecord)
            {
                if (index != 0)
                    throw new IndexOutOfRangeException($"index: {index} (expected: 0)");

                SetSection(section, null);
            }

            List<IDnsRecord> recordList = (List<IDnsRecord>)records;
            recordList.RemoveAt(index);
        }

        /// <summary>
        /// The SetRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        public void SetRecord(DnsSection section, IDnsRecord record)
        {
            Clear(section);
            SetSection(section, record);
        }

        /// <summary>
        /// The SetRecord
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        public void SetRecord(DnsSection section, int index, IDnsRecord record)
        {
            CheckQuestion(section, record);

            object records = SectionAt(section);
            if (records == null)
                throw new IndexOutOfRangeException($"index: {index} (expected: none)");

            if (records is IDnsRecord)
            {
                if (index == 0)
                {
                    SetSection(section, record);
                }
                else
                {
                    throw new IndexOutOfRangeException($"index: {index} (expected: 0)");
                }
            }

            List<IDnsRecord> recordList = (List<IDnsRecord>)records;
            recordList[index] = record;
        }

        /// <summary>
        /// The Touch
        /// </summary>
        /// <param name="hint">The hint<see cref="object"/></param>
        /// <returns>The <see cref="IReferenceCounted"/></returns>
        public override IReferenceCounted Touch(object hint)
        {
            if (leak != null)
                leak.Record(hint);

            return this;
        }

        /// <summary>
        /// The Deallocate
        /// </summary>
        protected override void Deallocate()
        {
            Clear();
            if (leak != null)
                leak.Close(this);
        }

        /// <summary>
        /// The CheckQuestion
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="record">The record<see cref="IDnsRecord"/></param>
        private static void CheckQuestion(DnsSection section, IDnsRecord record)
        {
            if (section == SECTION_QUESTION &&
                record != null &&
                !(record is IDnsQuestion))
                throw new ArgumentException($"record: {record} (expected: DnsQuestion)");
        }

        /// <summary>
        /// The SectionAt
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <returns>The <see cref="object"/></returns>
        private object SectionAt(DnsSection section)
        {
            switch (section)
            {
                case DnsSection.QUESTION:
                    return questions;

                case DnsSection.ANSWER:
                    return answers;

                case DnsSection.AUTHORITY:
                    return authorities;

                case DnsSection.ADDITIONAL:
                    return additionals;

                default:
                    return null;
            }
        }

        /// <summary>
        /// The SetSection
        /// </summary>
        /// <param name="section">The section<see cref="DnsSection"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        private void SetSection(DnsSection section, object value)
        {
            switch (section)
            {
                case DnsSection.QUESTION:
                    questions = value;
                    break;

                case DnsSection.ANSWER:
                    answers = value;
                    break;

                case DnsSection.AUTHORITY:
                    authorities = value;
                    break;

                case DnsSection.ADDITIONAL:
                    additionals = value;
                    break;
            }
        }

        #endregion 方法
    }
}