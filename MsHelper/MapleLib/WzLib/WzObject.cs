﻿/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System; 
using MsHelper.MapleLib.WzLib.Util;

 namespace MsHelper.MapleLib.WzLib
{
    /// <summary>
    /// An abstract class for wz objects
    /// </summary>
    public abstract class WzObject : IDisposable
    {
        public abstract void Dispose();

        /// <summary>
        /// The name of the object
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        public abstract WzObject Parent { get; internal set; }

        /// <summary>
        /// Returns the parent WZ File
        /// </summary>
        public abstract WzFile WzFileParent { get; }

        public WzObject this[string name]
        {
            get
            {
                if (this is WzFile) return ((WzFile) this)[name];
                if (this is WzDirectory) return ((WzDirectory) this)[name];
                if (this is WzImage) return ((WzImage) this)[name];
                if (this is WzImageProperty) return ((WzImageProperty) this)[name];
                throw new NotImplementedException();
            }
        }

        public string FullPath
        {
            get
            {
                if (this is WzFile) return ((WzFile) this).WzDirectory.Name;
                var result = this.Name;
                var currObj = this;
                while (currObj.Parent != null)
                {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }

                return result;
            }
        }

        /// <summary>
        /// Used in HaCreator to save already parsed images
        /// </summary>
        public object HcTag { get; set; }

        /// <summary>
        /// Used in HaCreator's MapSimulator to save already parsed textures
        /// </summary>
        public object MsTag { get; set; }

        /// <summary>
        /// Used in HaRepacker to save WzNodes
        /// </summary>
        public object HrTag { get; set; }

        public virtual object WzValue => null;

        //Credits to BluePoop for the idea of using cast overriding
        //2015 - That is the worst idea ever, removed and replaced with Get* methods

        #region Cast Values

        public virtual int GetInt()
        {
            throw new NotImplementedException();
        }

        public virtual short GetShort()
        {
            throw new NotImplementedException();
        }

        public virtual long GetLong()
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat()
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual WzVector2 Pos()
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        #endregion

        public static implicit operator int(WzObject wzObject)
        {
            return wzObject?.GetInt() ?? 0;
        }
    }
}