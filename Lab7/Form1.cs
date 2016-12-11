/* Elizabeth Kenny
 * EC447 Lab 7
 * Due November 30, 2016
 * *****************************************
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace Lab7
{
    public partial class Form1 : Form
    {
        public byte[] key;                      // 8 bit key
        public string fOutName;                 // output file name

        public Form1()
        {
            InitializeComponent();
            Text = "Lab 7 File Encryptor/Decryptor by Elizabeth Kenny";
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            if (!isEncryptError())
            {
                setKey();
                if (!checkOverwrite())                              // file name OK && overwrite validated
                {
                    //Create the file streams to handle the input and output files.
                    FileStream fin = null;
                    FileStream fout = null;

                    try                                             // open files/streams
                    {
                        fin = new FileStream(textBoxFileName.Text, FileMode.Open, FileAccess.Read);
                        fout = new FileStream(fOutName, FileMode.OpenOrCreate, FileAccess.Write);
                        fout.SetLength((long)0);
                    }
                    catch                                           // if streams fail          
                    {
                        MessageBox.Show("Could not open source or destination file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (fin != null)
                            fin.Close();
                        if (fout != null)
                            fout.Close();
                        return;
                    }
                    //Create variables to help with read and write.
                    byte[] bin = new byte[100];                               //This is intermediate storage for the encryption.
                    long rdlen = (long)0;                                     //This is the total number of bytes written.
                    long totlen = fin.Length;                                 //This is the total length of the input file.
                    // encryption stream
                    DES des = new DESCryptoServiceProvider();
                    CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(key, key), CryptoStreamMode.Write);
                    //Read from the input file, then encrypt and write to the output file.
                    while (rdlen < totlen)
                    {
                        int len = fin.Read(bin, 0, 100);                      //This is the number of bytes to be written at a time.
                        encStream.Write(bin, 0, len);
                        rdlen = rdlen + (long)len;
                    }

                    encStream.Close();
                    fout.Close();
                    fin.Close();
                }
                else
                    return;
            }
            else
                return;
        }
        // decryption triggered with button click
        private void decryptButton_Click(object sender, EventArgs e)
        {
            if (!isDecryptError())                  // no file or key errors
            {
                setKey();                           // set 8 bit key
                if (!checkOverwrite())              // file name OK && overwrite validated
                {
                    //Create the file streams to handle the input and output files.
                    FileStream fin = null;
                    FileStream fout = null;
                    try                    // file streams
                    {
                        fin = new FileStream(textBoxFileName.Text, FileMode.Open, FileAccess.Read);
                        fout = new FileStream(fOutName, FileMode.OpenOrCreate, FileAccess.Write);
                        fout.SetLength((long)0);
                    }
                    catch
                    {
                        MessageBox.Show("Could not open source or destination file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        if (fin != null)
                            fin.Close();
                        if (fout != null)
                            fout.Close();
                        return;
                    }
                    //Create variables to help with read and write.
                    byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
                    long rdlen = (long)0;              //This is the total number of bytes written.
                    long totlen = fin.Length;    //This is the total length of the input file.
                    // init decryption stream
                    DES des = new DESCryptoServiceProvider();
                    CryptoStream decStream = new CryptoStream(fout, des.CreateDecryptor(key, key), CryptoStreamMode.Write);             // create decryptor instead of encryptor
                    bool decryptFail = false;
                    try     // put encrpytion while loop in try/catch statement and decrypt instead
                    {
                        while (rdlen < totlen)
                        {
                            int len = fin.Read(bin, 0, 16);             // 16 instead of 100 to decrypt
                            decStream.Write(bin, 0, len);
                            rdlen = rdlen + (long)len;
                        }
                        decStream.Close();                              // close stream when done
                    }
                    catch                                               // if can't decrypt, delete init'd outFile and close filestreams
                    {
                        MessageBox.Show("Bad key or file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        decryptFail = true;
                    }
                    decStream.Close();                                  // close stream when done
                    fout.Close();
                    fin.Close();
                    if (decryptFail)
                        File.Delete(fOutName);
                }
                else
                    return;
            }
            else
                return;
        }
        // open file via file dialog w/ button click
        private void fileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDia = new OpenFileDialog();
            if (fileDia.ShowDialog() == DialogResult.OK)
                textBoxFileName.Text = fileDia.FileName;           // set textbox as file name when OK is pressed
        }
        // check for file or key errors prior to encryption
        private bool isEncryptError()
        {
            if (textBoxKey.Text == "")                  // error check for no key entered
            {
                MessageBox.Show("Please enter a key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            if (textBoxFileName.Text == "")             // errror check for no file name entered
            {
                MessageBox.Show("Could not open source or destination file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            if (textBoxFileName.Text.EndsWith(".des"))      // file already encrypted
            {
                if (MessageBox.Show("This file is already encrypted. Continue?", "Encryption encountered", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        // check for file or key errors prior to decryption
        private bool isDecryptError()
        {
            if (textBoxKey.Text == "")                  // no key entered
            {
                MessageBox.Show("Please enter a key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            if (textBoxFileName.Text == "" || !textBoxFileName.Text.EndsWith(".des"))             // no file name entered
            {
                MessageBox.Show("Not a .des file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            else
                return false;
        }
        // set output file name (fOutName) and verify if overwrite is OK
        private bool checkOverwrite()
        {
            fOutName = textBoxFileName.Text.Substring(0, textBoxFileName.Text.Length - 3);
            if (File.Exists(fOutName))                  // check if name exists, ask if overwrite is gucci
            {
                if (MessageBox.Show("Output file exists. Overwrite?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        // check and set key
        private void setKey()
        {
            key = new byte[8];
            int i = 0;
            for (int j = 0; j < textBoxKey.Text.Length; j++)
            {
                key[i] = (byte)(key[i] + (byte)this.textBoxKey.Text[j]);
                i = ((i + 1) % 8);
            }
        }
    }
}
