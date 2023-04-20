from flask import Flask, request, jsonify
import pickle
import numpy as np

app = Flask(__name__)

# Load your SVM model
with open('svm_model.pkl', 'rb') as file:
    svm_model = pickle.load(file)

@app.route('/predict', methods=['POST'])
def predict():
    input_data = request.get_json()  # Get input data from the POST request
    print("Received data) #: ", input_data)
    input_data = np.array(input_data['items'])
    # print("Input data np: ", input_data)
    input_data = input_data.reshape(-1, 3)
    # add axis for preprocessing
    # from pre processing of training data
    target_len = 153
    # pre process test data
    print("Fixing length to 153, original length: ", str(input_data.shape[0]))
    input_data = prepare_data(input_data, target_len)
    print("New length: ", str(input_data.shape[0]))
    # align
    print("Normalizing")
    input_data = preprocess_point_cloud_np(input_data)
    # flatten
    input_data = input_data.reshape((-1))
    print("Flattened to: ", str(input_data.shape))
    # predict
    print("Reshape & Predicting")
    predictions = svm_model.predict(input_data.reshape((1,-1))).tolist()  # Run predictions on the array of input data and convert it to a list
    print("Returning json predicts: ", jsonify(predictions).get_data(as_text=True))
    return jsonify(predictions)  # Return the predictions as a JSON response

def random_sampling(sequence, target_length):
    current_length = sequence.shape[0]
    indices = np.random.choice(current_length, target_length, replace=False)
    indices.sort()
    return sequence[indices]
def nearest_neighbor_resampling(sequence, target_length):
    current_length = sequence.shape[0]
    indices = np.round(np.linspace(0, current_length - 1, target_length)).astype(int)
    return sequence[indices]
def prepare_data(arr, average_len):
    average_sequence_length = average_len
    i = 0
    current_length = arr.shape[0]
    if current_length < average_sequence_length:
        resampled_sequence = nearest_neighbor_resampling(arr, average_sequence_length)
    elif current_length > average_sequence_length:
        resampled_sequence = random_sampling(arr, average_sequence_length)
    else:
        resampled_sequence = arr
    return resampled_sequence

def preprocess_point_cloud_np(point_cloud):
    # Translate the centroid of the point cloud to the origin
    centroid = np.mean(point_cloud, axis=0)
    point_cloud = point_cloud - centroid

    # Scale the point cloud to a fixed maximum size (e.g., 1 unit)
    max_size = 1.0
    distances = np.linalg.norm(point_cloud, axis=1)
    max_distance = np.max(distances)
    point_cloud = point_cloud * (max_size / max_distance)

    return point_cloud
if __name__ == '__main__':
    app.run()
    