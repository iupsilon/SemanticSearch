from langchain.callbacks.manager import CallbackManager
from langchain.callbacks.streaming_stdout import StreamingStdOutCallbackHandler
from langchain.chains import RetrievalQA
from langchain.llms import LlamaCpp

from langchain.vectorstores.pgvector import PGVector
from langchain.embeddings import HuggingFaceEmbeddings
from langchain.docstore.document import Document
from langchain.prompts import PromptTemplate
from langchain.chains import ConversationalRetrievalChain

embeddings = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")


CONNECTION_STRING = PGVector.connection_string_from_db_params("psycopg2","localhost",5432,"AIDB","ai","qwerty,123")
COLLECTION_NAME = "BOOKS"

vectorStore = PGVector(
    collection_name=COLLECTION_NAME,
    connection_string=CONNECTION_STRING,
    embedding_function=embeddings,
)

# Callbacks support token-wise streaming
callback_manager = CallbackManager([StreamingStdOutCallbackHandler()])

# llm = LlamaCpp(
#     model_path="models/llama-2-7b-chat.Q2_K.gguf",
#     # model_path="/home/yari/.cache/lm-studio/models/TheBloke/Llama-2-7B-Chat-GGUF/llama-2-7b-chat.Q8_0.gguf",
#     temperature=0.75,
#     max_tokens=2000,
#     top_p=1,
#     callback_manager=callback_manager,
#     verbose=True
# )

# Prompt: https://github.com/langchain-ai/langchain/issues/11014
prompt_template = """
Film trovato:
{context} 

Descrivi il racconto in italiano.

"""
PROMPT = PromptTemplate(template=prompt_template, input_variables=["context", "question"])

retriever = vectorStore.as_retriever(search_type="similarity_score_threshold", search_kwargs={"k": 1, "score_threshold": 0.4})


# qa = RetrievalQA.from_chain_type(
#     llm,
#     chain_type="stuff",
#     return_source_documents=True,
#     retriever=retriever,
#     chain_type_kwargs={"prompt": PROMPT}
# )

# qb = ConversationalRetrievalChain.from_llm(
#     llm,
#     retriever,
#     combine_docs_chain_kwargs={"prompt": PROMPT},
#     return_source_documents=True,
#     verbose=True
# )


# Execute the chain
while True:
    user_input = input("Inserisci una stringa ('end' per terminare): ")

    if user_input.lower() == 'end':
        print("Hai scritto 'end'. Uscita.")
        break
    else:
        print(f"Hai inserito: {user_input}")
        docs = retriever.get_relevant_documents(query=user_input)
        print(docs)
        #if len(docs) > 0:
            #qa.invoke(user_input)
            #qb.invoke({"question": user_input, "chat_history": ""})
