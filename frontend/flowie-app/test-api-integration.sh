#!/bin/bash

# E2E API Integration Test Script
# This script tests the API endpoints to verify the integration works correctly
#
# Prerequisites:
# - Backend must be running on http://localhost:5000
# - Database should be seeded with test data

API_URL="http://localhost:5000"
PASS_COUNT=0
FAIL_COUNT=0

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================="
echo "  Flowie API Integration E2E Tests"
echo "========================================="
echo ""

# Check if backend is running
echo -n "Checking if backend is running... "
if curl -s -o /dev/null -w "%{http_code}" "$API_URL/api/projects" | grep -q "200"; then
    echo -e "${GREEN}✓ Backend is running${NC}"
else
    echo -e "${RED}✗ Backend is not running on $API_URL${NC}"
    echo "Please start the backend with: cd backend/Flowie.Api && dotnet run"
    exit 1
fi

echo ""

# Test function
test_endpoint() {
    local test_name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    
    echo -n "Testing: $test_name... "
    
    if [ "$method" = "GET" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL$endpoint")
    elif [ "$method" = "POST" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" \
            -X POST "$API_URL$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data")
    elif [ "$method" = "PUT" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" \
            -X PUT "$API_URL$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data")
    elif [ "$method" = "DELETE" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" \
            -X DELETE "$API_URL$endpoint")
    fi
    
    if echo "$expected_status" | grep -q "$status"; then
        echo -e "${GREEN}✓ PASS${NC} (Status: $status)"
        ((PASS_COUNT++))
    else
        echo -e "${RED}✗ FAIL${NC} (Expected: $expected_status, Got: $status)"
        ((FAIL_COUNT++))
    fi
}

echo "Testing GET Endpoints"
echo "---------------------"

# Test 1: Get all projects
test_endpoint "GET all projects" "GET" "/api/projects" "" "200"

# Test 2: Get projects filtered by company (Immoseed = 0)
test_endpoint "GET projects filtered by company" "GET" "/api/projects?company=0" "" "200"

# Test 3: Get project by ID (assuming project 1 exists)
test_endpoint "GET project by ID" "GET" "/api/projects/1" "" "200|404"

# Test 4: Get all task types
test_endpoint "GET all task types" "GET" "/api/task-types" "" "200"

# Test 5: Get tasks for a project (assuming project 1 exists)
test_endpoint "GET tasks for project" "GET" "/api/tasks?projectId=1&onlyShowMyTasks=false" "" "200"

echo ""
echo "Testing POST Endpoints"
echo "----------------------"

# Test 6: Create a new project
PROJECT_DATA='{
  "title": "E2E Test ProjectModel",
  "description": "Created by automated e2e test",
  "company": 0
}'
test_endpoint "POST create project" "POST" "/api/projects" "$PROJECT_DATA" "200|201|204"

# Test 7: Create a new task type
TASKTYPE_DATA='{
  "name": "E2E Test Type"
}'
test_endpoint "POST create task type" "POST" "/api/task-types" "$TASKTYPE_DATA" "200|201|204"

# Note: Creating a task requires valid IDs for project, taskType, and employee
# These tests assume the data exists from seeding

echo ""
echo "Testing Error Handling"
echo "----------------------"

# Test 8: Invalid endpoint
test_endpoint "GET non-existent endpoint" "GET" "/api/invalid-endpoint" "" "404"

# Test 9: Create project with invalid data (empty body)
test_endpoint "POST project with invalid data" "POST" "/api/projects" "{}" "400"

echo ""
echo "========================================="
echo "  Test Summary"
echo "========================================="
echo -e "${GREEN}Passed: $PASS_COUNT${NC}"
echo -e "${RED}Failed: $FAIL_COUNT${NC}"
echo ""

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "${GREEN}✓ All tests passed!${NC}"
    echo ""
    echo "The API integration is working correctly."
    echo "You can now test the frontend at http://localhost:4200"
    exit 0
else
    echo -e "${RED}✗ Some tests failed!${NC}"
    echo ""
    echo "Please check the backend logs and ensure:"
    echo "  1. Backend is running correctly"
    echo "  2. Database is seeded with test data"
    echo "  3. CORS is configured to allow requests from the frontend"
    exit 1
fi
