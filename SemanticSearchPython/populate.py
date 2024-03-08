from langchain.vectorstores.pgvector import PGVector
from langchain.embeddings import HuggingFaceEmbeddings
from langchain.docstore.document import Document
from langchain.text_splitter import RecursiveCharacterTextSplitter

embeddings = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")

CONNECTION_STRING = PGVector.connection_string_from_db_params("psycopg2","localhost",5432,"AIDB","ai","qwerty,123")
COLLECTION_NAME = "BOOKS"


store = PGVector(
    collection_name=COLLECTION_NAME,
    connection_string=CONNECTION_STRING,
    embedding_function=embeddings,
)

from langchain_community.document_loaders.csv_loader import CSVLoader
csvLoader = CSVLoader(file_path="..\\Dataset\\books.csv", encoding="utf8")
text_splitter = RecursiveCharacterTextSplitter(chunk_size = 2000, chunk_overlap = 0)
dataCsv = csvLoader.load_and_split(text_splitter=text_splitter)
store.add_documents(dataCsv)

