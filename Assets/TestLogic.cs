using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GameOfLife
{
    public class TestLogic : MonoBehaviour
    {
        [SerializeField]
        private ulong testValue = 0b1111111100000011000000000001000000111000000100000000000000000000;

        // Start is called before the first frame update
        void Start()
        {
            TestLogicFunc();
        }

        private void TestLogicFunc()
        {
            var rightShift = testValue >> 8;
            var leftShift = testValue << 8;


            Debug.Log(SpliceText(testValue));
            //move up
            Debug.Log(SpliceText(leftShift));
            //move down
            Debug.Log(SpliceText(rightShift));
            var byteArray = BitConverter.GetBytes(testValue);
            Byte[] byteArray2 = new byte[byteArray.Length];
            Debug.Log(byteArray.Length);
            byteArray.CopyTo(byteArray2,0);
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i]=(byte)((int)byteArray[i] >> 1);
            }
            for (int i = 0; i < byteArray2.Length; i++)
            {
                byteArray2[i]=(byte)((int)byteArray2[i] << 1);
            }

            ulong upShift = BitConverter.ToUInt64(byteArray, 0);
            ulong downShift = BitConverter.ToUInt64(byteArray2, 0);
            //move left
            Debug.Log(SpliceText(upShift));
            //move right
            Debug.Log(SpliceText(downShift));
            ulong rightLeft = rightShift & leftShift;
            ulong upDown = upShift & downShift;
            ulong upLeft = upShift & leftShift;
            ulong upRight = upShift & rightShift;
            ulong downLeft = downShift & leftShift;
            ulong downRight = downShift & rightShift;
            ulong test2Neighbors = (upDown ^ upLeft ^ upRight ^ downLeft ^ downRight ^ rightLeft) & testValue;
            ulong test3Neighbors = upDown & (leftShift ^ rightShift) | rightLeft & (upShift ^ downShift);
            Debug.Log(SpliceText(test2Neighbors | test3Neighbors));

        }
        public static string SpliceText1(string text, int lineLength) {
            return Regex.Replace(text, "(.{" + lineLength + "})", "$1" + Environment.NewLine);
        }
        public static string SpliceText2(string text, int lineLength) {
            return Regex.Replace(text, "(.{" + lineLength + "})", "$1 \t");
        }
        public static string SpliceText(string text) {
            return SpliceText2(SpliceText1(text,8),1);
        }
        public static string SpliceText(ulong value) {
            var text = Convert.ToString((long)value, 2).PadLeft(64,'0');
            return SpliceText(text);
        }
    }
}


