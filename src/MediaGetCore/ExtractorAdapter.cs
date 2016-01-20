﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaGetCore{
    public class ExtractorAdapter : ExtractorBase {
        public override event ProcessEvent OnProcess;
        public override event CompletedEvent OnCompleted;
        /// <summary>
        /// 支援對象剖析器類型
        /// </summary>
        public List<Type> EntityList { get; set; } = new List<Type>();
        
        /// <summary>
        /// 建構剖析器的類型
        /// </summary>
        /// <param name="Extractors">初始化時加入的支援項目</param>
        public ExtractorAdapter(params Type[] Extractors) {
            this.EntityList.AddRange(Extractors);
        }

        public override bool IsMatch(string Url) => this.EntityList.Select(item => (IExtractor)Activator.CreateInstance(item)).ToList().Exists(item => item.IsMatch(Url));
                
        public override async Task<MediaInfo[]> GetMediaInfosAsync(Uri Url) {
            IExtractor extractor = (from t in this.EntityList.Select(item=>Activator.CreateInstance(item) as IExtractor) where (t?.IsMatch(Url) ?? false) select t).FirstOrDefault();
            if (extractor == null) throw new NotSupportedException("配接器內未含有支援項目實體");

            extractor.OnProcess += this.OnProcess;
            extractor.OnCompleted += this.OnCompleted;

            return await extractor.GetMediaInfosAsync(Url);
        }
    }
}