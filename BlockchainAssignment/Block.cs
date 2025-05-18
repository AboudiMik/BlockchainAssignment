using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;  
using System.Diagnostics;


namespace BlockchainAssignment
{
    class Block
    {
        private int index,
            difficulty = 4;
        private DateTime timestamp;
        public String prevhash,
            hash,
            merkleRoot,
            minerAddress;


        public List<Transaction> transactionList;

        public long nonce = 0;

        public long miningTimeMs { get; private set; }

        public double reward = 1.0;
        public double fees = 0.0;

     
        public Block()
        {
           timestamp = DateTime.Now;
            index = 0;
            hash = Mine();
            reward = 0;
            transactionList = new List<Transaction>();
        }

        public Block(Block lastBlock, List<Transaction> transactions, String address, int difficultyLevel)
        {
            timestamp = DateTime.Now;

            index = lastBlock.index + 1;
            prevhash = lastBlock.hash;

            this.minerAddress = address; // The wallet to be credited the reward for the mining effort
            reward = 1.0; // Assign a simple fixed value reward
            transactions.Add(CreateRewardTransaction(transactions)); // Create and append the reward transaction
            transactionList = new List<Transaction>(transactions); // Assign provided transactions to the block
            this.difficulty = difficultyLevel; // Set the difficulty level for this block
            merkleRoot = MerkleRoot(transactionList); // Calculate the merkle root of the blocks transactions
            foreach (Transaction t in new List<Transaction>(transactionList))
            {
                if (!string.IsNullOrEmpty(t.smartContractCode))
                {
                    ExecuteSmartContract(t); // Call the smart contract executor
                }
            }
            hash = Mine();
        }

        public String CreateHash()
        {
            String hash = String.Empty;

            SHA256 hasher = SHA256Managed.Create();
            String input = index.ToString() + timestamp.ToString() + prevhash + nonce.ToString();

            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }

            return hash;
        }

        public Transaction CreateRewardTransaction(List<Transaction> transactions)
        {
            fees = transactions.Aggregate(0.0, (acc, t) => acc + t.fee);
            return new Transaction("Mine Rewards", minerAddress, (reward + fees ), 0, "", "");
        }
        public String Mine()
        {
            int threadCount = Environment.ProcessorCount;
            string targetPrefix = new string('0', difficulty);
            string finalHash = null;
            long foundNonce = -1;
            bool found = false;

            object lockObj = new object();
            Stopwatch sw = Stopwatch.StartNew();

            void MineThread(object obj)
            {
                int threadId = (int)obj;
                long localNonce = threadId;

                while (!found)
                {
                    nonce = localNonce;
                    string hash = CreateHash();

                    if (hash.StartsWith(targetPrefix))
                    {
                        lock (lockObj)
                        {
                            if (!found)
                            {
                                found = true;
                                finalHash = hash;
                                foundNonce = localNonce;
                            }
                        }
                        return;
                    }

                    localNonce += threadCount;
                }
            }

            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(MineThread);
                threads[i].Start(i);
            }

            for (int i = 0; i < threadCount; i++)
                threads[i].Join();

            nonce = foundNonce;
            sw.Stop();
            miningTimeMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"Mining took {sw.ElapsedMilliseconds} ms with {threadCount} threads.");
            return finalHash;
            

        }

        public static String MerkleRoot(List<Transaction> transactionList)
        {
            List<String> hashes = transactionList.Select(t => t.hash).ToList(); // Get a list of transaction hashes for "combining"

            // Handle Blocks with...
            if (hashes.Count == 0) // No transactions
            {
                return String.Empty;
            }
            if (hashes.Count == 1) // One transaction - hash with "self"
            {
                return HashCode.HashTools.CombineHash(hashes[0], hashes[0]);
            }
            while (hashes.Count != 1) // Multiple transactions - Repeat until tree has been traversed
            {
                List<String> merkleLeaves = new List<String>(); // Keep track of current "level" of the tree

                for (int i = 0; i < hashes.Count; i += 2) // Step over neighbouring pair combining each
                {
                    if (i == hashes.Count - 1)
                    {
                        merkleLeaves.Add(HashCode.HashTools.CombineHash(hashes[i], hashes[i])); // Handle an odd number of leaves
                    }
                    else
                    {
                        merkleLeaves.Add(HashCode.HashTools.CombineHash(hashes[i], hashes[i + 1])); // Hash neighbours leaves
                    }
                }
                hashes = merkleLeaves; // Update the working "layer"
            }
            return hashes[0]; // Return the root node
        }

        private void ExecuteSmartContract(Transaction t)
        {
            // Very basic example — you can extend this
            if (t.smartContractCode == "BONUS_IF_OVER_100")
            {
                if (t.amount > 100)
                {
                    Transaction bonus = new Transaction("SmartContract", t.recipientAddress, 10.0, 0, "", ""); // Add bonus
                    transactionList.Add(bonus);
                }
            }
            else if (t.smartContractCode.StartsWith("REDIRECT_50%_TO_"))
            {
                string[] parts = t.smartContractCode.Split('_');
                string redirectAddress = parts[3];

                double redirectAmount = t.amount * 0.5;
                Transaction redirected = new Transaction(t.senderAddress, redirectAddress, redirectAmount, 0, "", "");
                transactionList.Add(redirected);
            }
        }


        public override string ToString()
        {

            return "[BLOCK START]"
                + " Block Index: " + index.ToString()
                + "\nTimestamp: " + timestamp.ToString()
                + "\nPrevious Hash: " + prevhash
                + "\nHash: " + hash
                + "\n--PoW--"
                + "\nNonce: " + nonce.ToString()
                + "\nDifficulty: " + difficulty.ToString()
                + "\n--Rewards--"
                + "\nReward: " + reward.ToString()
                + "\nFees: " + fees.ToString()
                + "\nMiner's Address: " + minerAddress
                + "\n-- " + transactionList.Count + " Transactions --"
                + "\nMerkle Root: " + merkleRoot
                + "\n" + String.Join("\n", transactionList)
                + "\n[BLOCK END]";
           
                
        }
    }

}