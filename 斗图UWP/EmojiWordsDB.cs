using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace 斗图UWP
{
    //private EmojiWordsDB myEmojiWordsDB;
    //private static String ERROR = "error";
    //private static String HEAD_KEY = "HEAD_KEY";
    //private static String END_KEY = "END_KEY";
    //myEmojiWordsDB = new EmojiWordsDB();
    //var headContentStr = myEmojiWordsDB.queryDb(HEAD_KEY);
    //var headContent = new itemContent(headContentStr);
    //var backKey = headContent.bKey;
    //        while (true)
    //        {
    //            if (backKey.Equals(END_KEY))
    //                return;
    //            var temp = myEmojiWordsDB.queryDb(backKey);
    //var nodeContentStr = new itemContent(temp);
    //backKey = nodeContentStr.bKey;
    //            WordsListSorce.Add(new EmojiUiContent()
    //{
    //    words = nodeContentStr.content
    //            });
    //        }

    // itemContent类定义
    class itemContent
    {
        private static string fKeyString = "fKey";
        private static string bKeyString = "bKey";
        private static string contentKeyString = "Content";

        public string fKey { set; get; }
        public string bKey { set; get; }
        public string content { set; get; }

        /// <summary>
        /// 根据string值JSON解析为各个元素赋值
        /// </summary>
        /// <param name="bulidString"></param>
        public itemContent(string bulidString)
        {
            JsonObject json = JsonObject.Parse(bulidString);
            fKey = json.GetNamedString(fKeyString);
            bKey = json.GetNamedString(bKeyString);
            content = json.GetNamedString(contentKeyString);
        }

        /// <summary>
        /// 将自身转化为序列化字符串
        /// </summary>
        /// <returns></returns>
        public String getString()
        {
            var json = new JsonObject();
            json.SetNamedValue(fKeyString, JsonValue.CreateStringValue(fKey));
            json.SetNamedValue(contentKeyString, JsonValue.CreateStringValue(content));
            json.SetNamedValue(bKeyString, JsonValue.CreateStringValue(bKey));
            var JsonContent = json.Stringify();
            return JsonContent;
        }

        /// <summary>
        /// 重载构造函数
        /// </summary>
        public itemContent(string fKeyTemp, string contentTemp, string bKeyTemp)
        {
            fKey = fKeyTemp;
            content = contentTemp;
            bKey = bKeyTemp;
        }
    }

    class EmojiWordsDB
    {
        //全局常量
        #region
        private static String ISFIRST = "isFirst";
        private static String TRUE = "TRUE";
        private static String ERROR = "error";
        private static String[] EmojiWordsList={"MDZZ","你咋不上天","Lumia","跟党走","我爱你","去死吧","楼上智障","我们走"};
        #endregion

        //头结点相关常量
        #region
        private static String HEAD_CONTENT = "HEAD_CONTENT";
        private static String HEAD_KEY = "HEAD_KEY";
        private static String HEAD_F_Key = "HEAD_F_KEY";
        #endregion

        //尾节点相关数据
        #region
        private static String END_CONTENT = "END_CONTENT";
        private static String END_B_Key = "END_B_KEY";
        private static String END_KEY = "END_KEY";
        #endregion

        // 数据库常量定义
        #region
        private static String DB_NAME = "EmojiWordDb.db";
        //HISTORY table
        private static String HISTORY_TABLE_NAME = "HistoryTable";
        private static String SQL_CREATE_HISTORY_TABLE = "CREATE TABLE IF NOT EXISTS " + HISTORY_TABLE_NAME + " (Key TEXT,Value TEXT);";
        private static String SQL_HISTORY_QUERY_VALUE = "SELECT Value FROM " + HISTORY_TABLE_NAME + " WHERE Key = (?);";
        private static String SQL_HISTORY_INSERT = "INSERT INTO " + HISTORY_TABLE_NAME + " VALUES(?,?);";
        private static String SQL_HISTORY_UPDATE = "UPDATE " + HISTORY_TABLE_NAME + " SET Value = ? WHERE Key = ?";
        private static String SQL_HISTORY_DELETE = "DELETE FROM " + HISTORY_TABLE_NAME + " WHERE Key = ?";
        #endregion

        private SQLiteConnection _connection;

        //构造方法
        /// <summary>
        /// 构造函数
        /// 功能：完成数据库的构造
        /// </summary>
        public EmojiWordsDB()
        {
            try
            {
                _connection = new SQLiteConnection(DB_NAME);
                //链接准备
                using (var statement = _connection.Prepare(SQL_CREATE_HISTORY_TABLE))
                {
                    statement.Step();
                }
                FIRST_INIT();
            }
            catch (Exception)
            {
            }
        }

        //私有方法
        #region

        /// <summary>
        /// 判断是否需要构建初始链表，在为构建的时候调用指定函数
        /// </summary>
        private void FIRST_INIT()
        {
            var IS_FIRST_IＮIT = false;
            ApplicationDataContainer setting = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (setting.Values.ContainsKey(ISFIRST))
            {
                if (!setting.Values[ISFIRST].ToString().Equals(TRUE))
                {
                    IS_FIRST_IＮIT = true;
                }
            }
            else
            {
                IS_FIRST_IＮIT = true;
            }
            if (IS_FIRST_IＮIT)
            {
                INSERT_HEAD_END();
            }
        }

        /// <summary>
        /// 构建初始链表
        /// </summary>
        private void INSERT_HEAD_END()
        {
            var headItemContent = new itemContent(HEAD_F_Key, HEAD_CONTENT, END_KEY);
            addIntoDb(HEAD_KEY, headItemContent);
            var endItemContent = new itemContent(HEAD_KEY, END_CONTENT, END_B_Key);
            addIntoDb(END_KEY, endItemContent);
            //第一次插入数据库
            foreach (var str in EmojiWordsList)
            {
                addIntoDb(str);
            }
            ApplicationDataContainer setting = Windows.Storage.ApplicationData.Current.LocalSettings;
            setting.Values[ISFIRST] = TRUE;
        }

        /// <summary>
        /// 更新指定Key在数据库对应的value(适用于更新头尾节点更好)
        /// </summary>
        /// <param name="key">指定的key</param>
        /// <param name="value">指定key更新的value(itemContent)</param>
        private void updataDb(string key, itemContent value)
        {
            var result = value.getString();
            using (var statement = _connection.Prepare(SQL_HISTORY_UPDATE))
            {
                statement.Bind(1, result);
                statement.Bind(2, key);
                statement.Step();
            }
        }

        /// <summary>
        /// 数据库删除重载函数，供内部调用
        /// </summary>
        private void DELECT_Db(string key)
        {
            using (var statement = _connection.Prepare(SQL_HISTORY_DELETE))
            {
                statement.Bind(1, key);
                statement.Step();
            }
        }

        /// <summary>
        /// 此函数只用来构建首尾节点时方可调用,插入itemContent的jsonString
        /// </summary>
        /// <param name="key">指定的key</param>
        /// <param name="value">需要插入的itemContent值</param>
        private bool addIntoDb(string key, itemContent value)
        {
            try
            {
                var finaResult = value.getString();
                using (var statement = _connection.Prepare(SQL_HISTORY_INSERT))
                {
                    statement.Bind(1, key);
                    statement.Bind(2, finaResult);
                    statement.Step();
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        //公有方法
        #region

        /// <summary>
        /// 头插法插入历史数据库
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值(uiItemContent)</param>
        /// 采用头插法
        public void addIntoDb(string value)
        {
            try
            {
                string key = value;
                ///获取head value
                var headValue = queryDb(HEAD_KEY);
                if (headValue.Equals(ERROR))
                {
                    return;
                }
                var headItemContent = new itemContent(headValue);

                ///保存old第二节点
                var OLD_NODE_KEY = headItemContent.bKey;

                ///构造itemContent
                var itemContentTemp = new itemContent(HEAD_KEY, value, OLD_NODE_KEY);

                ///插入新建节点
                addIntoDb(key, itemContentTemp);

                //更新头结点
                headItemContent.bKey = key;
                updataDb(HEAD_KEY, headItemContent);

                //更新当前Second节点
                var secStr = queryDb(OLD_NODE_KEY);
                if (!secStr.Equals(ERROR))
                {
                    var SecItemContent = new itemContent(secStr);
                    SecItemContent.fKey = key;
                    updataDb(OLD_NODE_KEY, SecItemContent);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 根据键返回对应的值
        /// </summary>
        /// <param name="key">值</param>
        /// <returns>返回查询结果，查询不到返回“error”</returns>
        public string queryDb(string key)
        {
            using (var statement = _connection.Prepare(SQL_HISTORY_QUERY_VALUE))
            {
                statement.Bind(1, key);
                SQLiteResult result = statement.Step();
                if (SQLiteResult.ROW == result)
                {
                    var value = statement[0] as String;
                    return value;
                }
                return ERROR;
            }
        }

        /// <summary>
        /// 删除指定的历史记录
        /// </summary>
        /// <param name="key">指定的key</param>
        public void delectDb(string key)
        {
            var stringTemp = queryDb(key);
            if (stringTemp.Equals(ERROR))
            {
                return;
            }
            var itemContentTemp = new itemContent(stringTemp);
            var fKeyTemp = itemContentTemp.fKey;
            var bKeyTemp = itemContentTemp.bKey;

            //删除数据库
            DELECT_Db(key);

            //更新对应前节点的首节点的值
            var fNodeJsonValue = queryDb(fKeyTemp);
            if (!fNodeJsonValue.Equals(ERROR))
            {
                var fItemContentTemp = new itemContent(fNodeJsonValue);
                fItemContentTemp.bKey = bKeyTemp;
                updataDb(fKeyTemp, fItemContentTemp);
            }

            //更新对应尾节点的首节点的值
            var bNodeJsonValue = queryDb(bKeyTemp);
            if (!bNodeJsonValue.Equals(ERROR))
            {
                var bItemContentTemp = new itemContent(bNodeJsonValue);
                bItemContentTemp.fKey = fKeyTemp;
                updataDb(bKeyTemp, bItemContentTemp);
                var str = queryDb(bKeyTemp);
            }
        }

        #endregion
    }
}