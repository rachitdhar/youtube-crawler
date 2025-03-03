# youtube-crawler
A rudimentary crawler to retrieve youtube video URLs from all over youtube.

## Working

- The crawler maintains a queue of URLs that it has to crawl over (i.e., breadth-first search algorithm). Each crawled url is then appended in a urls.txt file.
- The crawling involves generating the dynamic HTML of the page for a given url. All new URLs found on that page are then added to the queue.
- In case there's any error in the process of generation of the HTML of a given url, the error is logged into a logs.txt file, and the program continues execution.
- To improve performance, a batch of urls are appended to the urls.txt file in one go, instead of appending after each crawl has completed.

## Instructions

- Set some limit for the maximum number of URLs to crawl over (default is taken as 1 million)
- Set the base URLs (one or more than one), which the crawler can begin crawling over
