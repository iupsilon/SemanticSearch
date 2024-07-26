from sentence_transformers import SentenceTransformer
from numpy import dot

text1 = 'sole'
text2 = 'felicità'

# define transofrmer model (from https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2)
#model = SentenceTransformer('sentence-transformers/gtr-t5-base')
model = SentenceTransformer('sentence-transformers/all-MiniLM-L6-v2')

vector1 = model.encode(text1)
vector2 = model.encode(text2)


#print(embedding)
result = dot(vector1, vector2)
print(result)
