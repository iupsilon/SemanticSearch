import csv

def save_first_n_rows(input_file, output_file, n):
    # Read the CSV file
    with open(input_file, 'r', newline='', encoding='utf-8') as file:
        reader = csv.reader(file)
        data = [row for row in reader]

    # Save the first n rows to a new CSV file
    with open(output_file, 'w', newline='', encoding='utf-8') as file:
        writer = csv.writer(file)
        writer.writerows(data[:n])

# Example usage:
input_file = '.\Dataset\movies_dataset.csv'
output_file = '.\Dataset\crop_movies_dataset.csv'
n = 500  # Change this value to the desired number of rows

save_first_n_rows(input_file, output_file, n)