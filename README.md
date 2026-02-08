# Pah-Pah

**Multiplayer hide & seek videogame used to demonstrate common cheating techniques in videogames.**

## Purpose

Pah-Pah is a simple multiplayer hide & seek game built to **showcase and analyze how players can cheat** in typical game scenarios. This repo contains the game project and examples of cheating vectors that can occur in networked games.

This is *not* meant to be a polished commercial game, but a product of my Bachelor's thesis. It is a demo/proof-of-concept for education, testing, and security research.

## Thesis
The thesis is made in Croatian language. Here is the abstract in English:

With an initial review of literature on networking and cheating in networked video games in the industry, tools and methods for cheating in such games were analyzed, and the most common vulnerabilities were demonstrated through a custom networked game developed in Unity. It was shown how tools like dnSpy allow code modification and exploitation of security flaws, such as point manipulation, wall hacks, and abuse of host privileges. Basic protection measures were proposed, including stronger serverside control and the use of anti-cheat software. The paper provides a practical overview of cheating issues and prevention strategies in modern networked games.

## Features

- Multiplayer hide & seek gameplay  
- Demonstrations of common cheating techniques (speed hacks, state tampering, etc.)  
- Unity project with networking logic  
- Basic assets and scenes to inspect and modify

## Requirements

- Unity (version used in development should be documented here)  
- A machine capable of running Unity Editor  
- (Optional) A network environment or local loopback for multiplayer testing

## Setup

1. **Clone the repository**
   ```sh
   git clone https://github.com/marinprusac/Pah-Pah.git
   cd Pah-Pah
