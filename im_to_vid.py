"""Image to video"""
import os
import sys
import cv2


def main():
    """args: output name, input folder """

    if len(sys.argv) < 3:
        print("args: output name, input folder")
        quit()

    image_folder = sys.argv[2]
    image_folder = '' + image_folder + ''
    video_name = sys.argv[1] + ".mp4"
    images = [img for img in os.listdir(image_folder) if img.endswith(".jpeg")]

    images.sort()

    for image in images:
        print(image)

    frame = cv2.imread(os.path.join(image_folder, images[0]))
    height, width, layers = frame.shape

    video = cv2.VideoWriter(
        video_name, cv2.VideoWriter_fourcc(*'mp4v'), 20, (width, height))

    for image in images:
        video.write(cv2.imread(os.path.join(image_folder, image)))

    cv2.destroyAllWindows()
    video.release()


main()
