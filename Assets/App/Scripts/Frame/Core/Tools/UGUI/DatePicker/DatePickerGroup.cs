/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月1 16:6
 * function    : 
 * ===============================================
 * */
/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年5月23 14:31
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    /// <summary>
    /// 日期选择组
    /// </summary>
    public class DatePickerGroup : MonoBehaviour
    {
        /// <summary>
        /// 最小日期和最大日期
        /// </summary>
        public DateTime _minDate, _maxDate;
        /// <summary>
        /// 选择的日期（年月日时分秒）
        /// </summary>
        public DateTime _selectDate;
        /// <summary>
        /// 时间选择器列表
        /// </summary>
        public List<DatePicker> _datePickerList;
        /// <summary>
        /// 当选择日期的委托事件
        /// </summary>
        public event OnDateUpdate _OnDateUpdate;

        public static DateTime _selectTime;
        void Awake()
        {
            _minDate = new System.DateTime(1970, 1, 1, 0, 0, 0);
            _maxDate = DateTime.Now;
            Init(_maxDate);
        }

        private void Update() { }
        public void Init(DateTime dt)
        {
            _selectDate = dt;
            for (int i = 0; i < _datePickerList.Count; i++)
            {
                _datePickerList[i].myGroup = this;
                _datePickerList[i].Init();
                _datePickerList[i]._onDateUpdate += onDateUpdate;
            }
        }
        public void Init()
        {
            _selectDate = DateTime.Now;
            for (int i = 0; i < _datePickerList.Count; i++)
            {
                _datePickerList[i].myGroup = this;
                _datePickerList[i].Init();
                _datePickerList[i]._onDateUpdate += onDateUpdate;
            }
        }

        public void GetConverTime(DateTime dt)
        {
            _selectDate = dt;
        }

        /// <summary>
        /// 当选择的日期更新
        /// </summary>
        public void onDateUpdate()
        {
            //Debug.Log("当前选择日期：" + _selectDate.ToString("yyyy年MM月dd日 HH : mm : ss"));
            //将选中的时间传入到创建任务脚本中
            //CreateNewTask.ChoiceTime = _selectDate;
            _selectTime = _selectDate;
            for (int i = 0; i < _datePickerList.Count; i++)
            {
                _datePickerList[i].RefreshDateList();
            }
        }
    }
}
