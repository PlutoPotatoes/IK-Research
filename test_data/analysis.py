import pandas as pd
from pandas import DataFrame
import numpy as np
import matplotlib


df = pd.read_csv('test2.csv', index_col='Frame')

LHMean = df['LeftArmDistance'].abs().mean()
RHMean = df['RightArmDistance'].abs().mean()
RLMean = df['RightLegDistance'].abs().mean()
LLMean = df['LeftLegDistance'].abs().mean()
SMean = df['ShouldersDistance'].abs().mean()
HMean = df['HeadDistance'].abs().mean()
LKMean = df['LeftKneeDistance'].abs().mean()
RKMean = df['RightKneeDistance'].abs().mean()
LEMean = df['LeftElbowDistance'].abs().mean()
REMean = df['RightElbowDistance'].abs().mean()





print(f"Left Hand: {LHMean}, Right Hand: {RHMean}, \nRight Leg: {RLMean}, Left Leg: {LLMean}, \nShoulders: {SMean}, Head: {HMean}")
print(f"Left Knee: {LKMean} \nRight Knee: {RKMean} \nLeft Elbow: {LEMean} \nRight Elbow: {REMean}")