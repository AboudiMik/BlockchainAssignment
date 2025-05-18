using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BlockchainAssignment
{
    public partial class BlockchainApp : Form
    {
        Blockchain blockchain;
        public BlockchainApp()
        {
            InitializeComponent();
            blockchain = new Blockchain();
            richTextBox1.Text = "New Blockchain initialised";
        }

       

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Duplicate the value input into our new textbox (textBox1) in the large "console" (richTextBox1)
        private void button1_Click(object sender, EventArgs e)
        {
            int index = 0;
            if(Int32.TryParse(textBox1.Text, out index))
            {
                richTextBox1.Text = blockchain.getBlockAsString(index);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            String privKey;
            Wallet.Wallet myNewWallet = new Wallet.Wallet(out privKey);
            privateKey.Text = myNewWallet.publicID;
            publicKey.Text = privKey;


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Wallet.Wallet.ValidatePrivateKey(publicKey.Text, privateKey.Text))
            {
                richTextBox1.Text = "Keys are Valid";
            }
            else
            {
                richTextBox1.Text = "Keys are invalid";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Transaction transaction = new Transaction(publicKey.Text, reciever.Text, Double.Parse(amount.Text), Double.Parse(fee.Text), privateKey.Text, smartContractCode.Text);
            blockchain.transactionPool.Add(transaction);
            richTextBox1.Text = transaction.ToString();
            

        }

        private void NewBlock_Click(object sender, EventArgs e)

        {
            List<Transaction> transactions = blockchain.getPendingTransactions();

            // Get current difficulty from the chain
            int currentDifficulty = blockchain.GetCurrentDifficulty();

            // Create a new block with adaptive difficulty
            Block newBlock = new Block(blockchain.GetLastBlock(), transactions, publicKey.Text, currentDifficulty);

            // Add the mined block to the blockchain
            blockchain.Blocks.Add(newBlock);

            // Update difficulty based on how long mining took
            int updatedDifficulty = blockchain.GetAdaptiveDifficulty(newBlock.miningTimeMs);

            // Update UI with block mining info and current state
            richTextBox1.Text = ( $"⛏️ Block mined in {newBlock.miningTimeMs} ms\n"
                + $"⚙️ Difficulty updated to: {updatedDifficulty}\n\n" 
                + blockchain.ToString());

        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = blockchain.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = String.Join("\n", blockchain.transactionPool);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = (blockchain.GetBalance(publicKey.Text).ToString() + " Assignment Coin");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (blockchain.Blocks.Count == 1)
            {
                if (!Blockchain.ValidateHash(blockchain.Blocks[0])) // Recompute Hash to check validity
                    richTextBox1.Text = ("Blockchain is invalid");
                else
                    richTextBox1.Text = ("Blockchain is valid");
                return;
            }

            for (int i = 1; i < blockchain.Blocks.Count - 1; i++)
            {
                if (
                    blockchain.Blocks[i].prevhash != blockchain.Blocks[i - 1].hash || // Check hash "chain"
                    !Blockchain.ValidateHash(blockchain.Blocks[i]) ||  // Check each blocks hash
                    !Blockchain.ValidateMerkleRoot(blockchain.Blocks[i]) // Check transaction integrity using Merkle Root
                )
                {
                    richTextBox1.Text = ("Blockchain is invalid");
                    return;
                }
            }
            richTextBox1.Text = ("Blockchain is valid"); if (blockchain.Blocks.Count == 1)
            {
                if (!Blockchain.ValidateHash(blockchain.Blocks[0])) // Recompute Hash to check validity
                    richTextBox1.Text = ("Blockchain is invalid");
                else
                    richTextBox1.Text = ("Blockchain is valid");
                return;
            }

            for (int i = 1; i < blockchain.Blocks.Count - 1; i++)
            {
                if (
                    blockchain.Blocks[i].prevhash != blockchain.Blocks[i - 1].hash || // Check hash "chain"
                    !Blockchain.ValidateHash(blockchain.Blocks[i]) ||  // Check each blocks hash
                    !Blockchain.ValidateMerkleRoot(blockchain.Blocks[i]) // Check transaction integrity using Merkle Root
                )
                {
                    richTextBox1.Text = ("Blockchain is invalid");
                    return;
                }
            }
            richTextBox1.Text = ("Blockchain is valid");
        }

      
    }

}
