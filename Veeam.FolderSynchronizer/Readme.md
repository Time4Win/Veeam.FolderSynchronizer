## File Comparison Performance Analysis

I implemented 2 file comparison algorithms:
1. **MD5 hash** (suggested in the task). Class **Md5HashFileComparison**. 
Assumption: comparing hash codes should use fewer resources than a byte-by-byte comparison. 
However, to build this hash, the entire file must still be read.
2. **Byte-by-Byte Stream Comparison**. Class **ByteByByteFileStreamsComparison** 
Instead of reading the whole file at once, it reads through a small buffer block by block and compares them. 
If it finds a difference, it immediately aborts the process with `AreFileEqual = false`. 
Therefore, it works significantly faster when changes occur near the beginning of the file, avoiding a full file read.

### Test Results (50 MB text file):

## Environment 
* **OS Name:** Microsoft Windows 11 Pro
* **Processor / CPU:** 13th Gen Intel(R) Core(TM) i7-13700H, 2400 Mhz, 14 Core(s), 20 Logical Processor(s)
* **RAM:** 32.0 GB

## Results & Conclusions
* **Identical files (Source and Replica are identical):**
  * **Byte-by-Byte:** ~0.1 sec
  * **MD5 Hash:** ~0.2 sec
  * *(Explanation: No CPU resources are spent on the heavy cryptographic math required to build an MD5 hash).*

* **File changed at the beginning (1 symbol changed, size remains the same):**
  * **Byte-by-Byte:** ~0.01 sec 
  * **MD5 Hash:** 0.2–0.3 sec

* **File changed in the middle or at the end of the file:**
  * **Byte-by-Byte:** ~0.1 sec 
  * **MD5 Hash:** 0.23–0.26 sec

### Summary & Recommendation:
Based on the performance tests, **Byte-by-Byte Stream Comparison** proves to be more efficient and faster for local folder synchronization, 
especially when changes occur at the beginning of files.