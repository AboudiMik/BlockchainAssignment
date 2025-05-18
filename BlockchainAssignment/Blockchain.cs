using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        public List<Block> Blocks = new List<Block>();

        public List<Transaction> transactionPool = new List<Transaction>();
        int transactionsPerBlock = 5;

        private int difficulty = 4; // Starting difficulty
        private readonly int targetBlockTime = 10000; // 10 seconds in milliseconds
        private DateTime lastBlockTime = DateTime.Now; // Timestamp to track previous block

        public Blockchain()
        {
            Blocks.Add(new Block());
        }

        public String getBlockAsString(int index)
        {
            if (index >= 0 && index < Blocks.Count)
                return Blocks[index].ToString(); // Return block as a string
            else
                return "No such block exists"; ;
        }

        public Block GetLastBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        public List<Transaction> getPendingTransactions()
        {
            int n = Math.Min(transactionsPerBlock, transactionPool.Count);
            List<Transaction> transactions = transactionPool.GetRange(0, n);
            transactionPool.RemoveRange(0, n);
            return transactions;
        }

        public static bool ValidateHash(Block b)
        {
            String rehash = b.CreateHash();
            return rehash.Equals(b.hash);
        }

        // Check validity of the merkle root by recalculating the root and comparing with the mined value
        public static bool ValidateMerkleRoot(Block b)
        {
            String reMerkle = Block.MerkleRoot(b.transactionList);
            return reMerkle.Equals(b.merkleRoot);
        }

        // Check the balance associated with a wallet based on the public key
        public double GetBalance(String address)
        {
            // Accumulator value
            double balance = 0;

            // Loop through all approved transactions in order to assess account balance
            foreach (Block b in Blocks)
            {
                foreach (Transaction t in b.transactionList)
                {
                    if (t.recipientAddress != null && t.recipientAddress.Equals(address))
                    {
                        balance += t.amount; // Credit funds recieved
                    }
                    if (t.senderAddress.Equals(address))
                    {
                        balance -= (t.amount + t.fee); // Debit payments placed
                    }
                }
            }
            return balance;
        }

        public int GetAdaptiveDifficulty(long lastMiningTimeMs)
        {
            if (lastMiningTimeMs < targetBlockTime * 0.9)
            {
                difficulty++; // Increase difficulty if mining was too fast
            }
            else if (lastMiningTimeMs > targetBlockTime * 1.1)
            {
                difficulty = Math.Max(1, difficulty - 1); // Decrease difficulty if mining was too slow
            }

            lastBlockTime = DateTime.Now;
            return difficulty;
        }

        public int GetCurrentDifficulty()
        {
            return difficulty;
        }
        public override string ToString()
        {
            return String.Join("\n", Blocks); 
        }
        
    }
}
