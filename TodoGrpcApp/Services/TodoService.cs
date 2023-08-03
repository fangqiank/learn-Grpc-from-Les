using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TodoGrpcApp.Data;
using TodoGrpcApp.Models;
using TodoGrpcApp.Protos;

namespace TodoGrpcApp.Services
{
    public class TodoService: TodoIt.TodoItBase
    {
        private readonly ILogger<TodoService> _logger;
        private readonly AppDbContext _db;

        public TodoService(ILogger<TodoService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public override async Task<CreateTodoResponse> CreateTodo(
            CreateTodoRequest request, 
            ServerCallContext context
            )
        {
            if (request.Title == string.Empty || request.Description == string.Empty)
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    "Invalid object"));

            var todoItem = new Todo
            {
                Title = request.Title,
                Description = request.Description,
            };

            await _db.AddAsync( todoItem );
            await _db.SaveChangesAsync();

            return await Task.FromResult( new CreateTodoResponse
            {
                Id = todoItem.Id,
            });
        }

        public override async Task<GetAllResponse> ListTodo(
            GetAllRequest request, 
            ServerCallContext context
            )
        {
            var response = new GetAllResponse();
            var todos = await _db.Todos.ToListAsync();

            foreach ( var todo in todos )
            {
                response.ToDo.Add(new ReadTodoResponse
                {
                    Id= todo.Id,
                    Title = todo.Title,
                    Description= todo.Description,
                    ToDoStatus = todo.ToDoStatus,
                });
            }

            return await Task.FromResult(response);
        }

        public override async Task<ReadTodoResponse> ReadTodo(
            ReadTodoRequest request, 
            ServerCallContext context
            )
        {
            if(request.Id <= 0)
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var todo = await _db.Todos.FirstOrDefaultAsync(x => x.Id == request.Id);

            if(todo != null)
            {
                return await Task.FromResult(new ReadTodoResponse
                {
                    Id = todo.Id,
                    Title= todo.Title,
                    Description = todo.Description,
                    ToDoStatus = todo.ToDoStatus,
                });
            }

            throw new RpcException(new Status(
                StatusCode.NotFound, $"No Task with id {request.Id}"));
        }

        public override async Task<UpdateTodoResponse> UpdateTodo(
            UpdateTodoRequest request, 
            ServerCallContext context
            )
        {
            if (request.Id <= 0
                || request.Title == string.Empty
                || request.Description == string.Empty
                )
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument, "Invalid object"));

            var updTodo = await _db.Todos.FirstOrDefaultAsync(x => x.Id == request.Id) ?? throw new RpcException(new Status(
                    StatusCode.NotFound, $"No Task with Id {request.Id}"));

            updTodo.Title = request.Title;
            updTodo.Description = request.Description;
            updTodo.ToDoStatus = request.ToDoStatus;

            await _db.SaveChangesAsync();

            return await Task.FromResult(new UpdateTodoResponse
            {
                Id = updTodo.Id,
            });
        }

        public override async Task<DeleteTodoResponse> DeleteTodo(
            DeleteTodoRequest request, 
            ServerCallContext context
            )
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var deleteTodo = await _db.Todos.FirstOrDefaultAsync(x => x.Id == request.Id) ?? throw new RpcException(new Status(
                    StatusCode.NotFound, $"No Task with Id {request.Id}"));

            _db.Remove(deleteTodo);

            await _db.SaveChangesAsync();

            return await Task.FromResult(new DeleteTodoResponse 
            { 
                Id = deleteTodo.Id, 
            });   
        }
    }
}
