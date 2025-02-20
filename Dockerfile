FROM node:latest AS build
WORKDIR /app

COPY package.json ./
COPY yarn.lock ./
COPY tsconfig.json ./
COPY src ./src
RUN yarn install && yarn run build

FROM node:latest AS runtime
WORKDIR /app
COPY --from=build /app/dist .
COPY --from=build /app/package*.json .
RUN yarn install --production

EXPOSE 3000
CMD ["node", "app.js"]