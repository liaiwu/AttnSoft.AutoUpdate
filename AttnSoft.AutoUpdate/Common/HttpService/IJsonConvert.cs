/*
 * 描述：
 * 作者：LAW  
 * 电子邮箱：315204916@qq.com
 * 时间：2022-05-07 0:27:58
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AttnSoft.AutoUpdate.Common
{
    public interface IJsonConvert
    {
        string Serialize(object obj);
        T Deserialize<T>(string jsonstr);
    }

}